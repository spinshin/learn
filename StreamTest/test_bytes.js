import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    vus: 100,
    duration: '30s',
};

export default function() {
    const res = http.get('http://localhost:5241/files/bytes', {
        responseType: 'binary',
    });

    check(res, {
        'bytes status is 200': (r) => r.status === 200,
        'bytes is not empty': (r) => r.body.length > 0,
    });
    
    // sleep(0.5);
}
