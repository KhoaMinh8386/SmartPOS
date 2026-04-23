const crypto = require('crypto');

function hashPasswordSha256(password) {
  return crypto.createHash('sha256').update(password, 'utf8').digest('hex');
}

const plainPassword = '12345';
const hashedPassword = hashPasswordSha256(plainPassword);

console.log('Plain:', plainPassword);
console.log('SHA256:', hashedPassword);
