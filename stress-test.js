import http from 'k6/http';
import { check, group, sleep } from 'k6';
import faker from 'https://cdnjs.cloudflare.com/ajax/libs/Faker/3.1.0/faker.min.js';

export const options = {
  stages: [
    { duration: '1m', target: 100 }, // normální 
    { duration: '5m', target: 500 }, // vyšší 
    { duration: '1m', target: 0 }, // cooldown
  ],
};

const BASE_URL = 'http://localhost:5241/User';

function createUserPayload() {
  return JSON.stringify({
    firstName: faker.name.firstName(),
    lastName: faker.name.lastName(),
    birthNumber: `${faker.random.number({ min: 1000, max: 9999 })}${faker.random.number({ min: 1000, max: 9999 })}`,
    dateOfBirth: faker.date.past().toISOString(),
  });
}

export default function () {
  group('Stress Test', () => {
    const postResponse = http.post(BASE_URL, createUserPayload(), { headers: { 'Content-Type': 'application/json' } });
    const locationHeader = postResponse.headers['Location'];
    const userId = locationHeader.split('/').pop();

    http.get(`${BASE_URL}/${userId}`);
    http.put(`${BASE_URL}/${userId}`, JSON.stringify({ id: userId,      firstName: faker.name.firstName(),	  lastName: faker.name.lastName(),    }), { headers: { 'Content-Type': 'application/json' } });
    http.get(`${BASE_URL}/${userId}/age`);
    http.del(`${BASE_URL}/${userId}`);
  });
  // sleep(1);
}