import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';
import { uuidv4 } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';

// Rajadas de requisições simultâneas: ROUNDS rodadas, cada uma com VUS VUs disparando 1 POST ao mesmo tempo.
// Cada rodada é um cenário próprio, iniciado com ROUND_GAP_SECONDS de intervalo entre uma e outra.
// A conta de cada requisição é sorteada entre ACCOUNT_MIN e ACCOUNT_MAX.
//
// Uso:
//   k6 run -e JWT="<token>" tests/k6/transactions-burst.js
//
// Variáveis opcionais:
//   BASE_URL           (padrão http://localhost:5107)
//   VUS                requisições simultâneas por rodada (padrão 100)
//   ROUNDS             número de rodadas (padrão 10)
//   ROUND_GAP_SECONDS  intervalo entre o início de cada rodada (padrão 5)
//   ACCOUNT_MIN        (padrão 1)
//   ACCOUNT_MAX        (padrão 10)
const VUS = Number(__ENV.VUS || 100);
const ROUNDS = Number(__ENV.ROUNDS || 10);
const ROUND_GAP_SECONDS = Number(__ENV.ROUND_GAP_SECONDS || 5);
const ACCOUNT_MIN = Number(__ENV.ACCOUNT_MIN || 1);
const ACCOUNT_MAX = Number(__ENV.ACCOUNT_MAX || 10);

const scenarios = {};
for (let i = 0; i < ROUNDS; i++) {
  scenarios[`round_${i + 1}`] = {
    executor: 'per-vu-iterations',
    vus: VUS,
    iterations: 1,
    startTime: `${i * ROUND_GAP_SECONDS}s`,
    maxDuration: `${ROUND_GAP_SECONDS}s`,
  };
}

export const options = {
  scenarios,
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<2000'],
    checks: ['rate>0.99'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5107';
const JWT = __ENV.JWT;
const TYPES = ['Credit', 'Debit'];

// Conta cada status HTTP recebido. Métricas precisam ser declaradas no init, então fixamos os esperados.
const statusCounters = {};
for (const code of [200, 201, 400, 401, 422, 500]) {
  statusCounters[code] = new Counter(`status_${code}`);
}
const statusOther = new Counter('status_other');
function countStatus(code) {
  (statusCounters[code] || statusOther).add(1);
}

export function setup() {
  if (!JWT) {
    throw new Error('Informe o token: k6 run -e JWT="<token>" tests/k6/transactions-burst.js');
  }
}

function randomAmount() {
  return Math.round((Math.random() * 999 + 1) * 100) / 100;
}

function randomAccountId() {
  return ACCOUNT_MIN + Math.floor(Math.random() * (ACCOUNT_MAX - ACCOUNT_MIN + 1));
}

export default function () {
  const body = {
    type: TYPES[Math.floor(Math.random() * TYPES.length)],
    accountId: randomAccountId(),
    amount: randomAmount(),
    createdBy: 'k6-load-test',
  };

  const res = http.post(`${BASE_URL}/api/transactions`, JSON.stringify(body), {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${JWT}`,
      'Idempotency-Key': uuidv4(), // chave única por requisição
    },
  });

  countStatus(res.status);

  // Sucesso é exclusivamente 201. Chave nova por requisição => nunca deve ser replay.
  check(res, {
    'status é 201': (r) => r.status === 201,
    'não é replay (sem Idempotent-Replayed)': (r) => !r.headers['Idempotent-Replayed'],
    'corpo com dados da transação': (r) => {
      try {
        const b = r.json();
        return b !== null && typeof b === 'object';
      } catch (e) {
        return false;
      }
    },
  });

  if (res.status !== 201) {
    console.error(`status=${res.status} body=${res.body}`);
  }
}
