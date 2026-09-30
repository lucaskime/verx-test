import http from 'k6/http';
import { check, sleep } from 'k6';
import { uuidv4 } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';

// 50 chamadas simultâneas (VUs), cada uma repetindo 100 vezes => 5000 requisições no total.
export const options = {
  scenarios: {
    transactions_load: {
      executor: 'per-vu-iterations',
      vus: 50,
      iterations: 100,
      maxDuration: '5m',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<1000'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5107';

const types = ['Credit', 'Debit'];

function randomAmount() {
  return Math.round((Math.random() * 999 + 1) * 100) / 100;
}

export default function () {
  const body = {
    type: types[Math.floor(Math.random() * types.length)],
    accountId: 1,
    amount: randomAmount(),
    createdBy: 'Lucas Mendes',
  };

  const res = http.post(`${BASE_URL}/api/transactions`, JSON.stringify(body), {
    headers: { 'Content-Type': 'application/json', 'Idempotency-Key': uuidv4() },
  });

  check(res, {
    'status é 200 ou 201': (r) => r.status === 200 || r.status === 201,
  });

  sleep(1);
}
