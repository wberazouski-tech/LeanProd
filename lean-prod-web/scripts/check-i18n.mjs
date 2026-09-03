import { readFile } from 'node:fs/promises';

const flatten = (value, prefix = '') => Object.entries(value).flatMap(([key, child]) => {
  const path = prefix ? `${prefix}.${key}` : key;
  return child && typeof child === 'object' && !Array.isArray(child) ? flatten(child, path) : [path];
});

const load = async language => JSON.parse(await readFile(new URL(`../src/assets/i18n/${language}.json`, import.meta.url), 'utf8'));
const [en, be] = await Promise.all([load('en'), load('be')]);
const enKeys = new Set(flatten(en));
const beKeys = new Set(flatten(be));
const missingInBe = [...enKeys].filter(key => !beKeys.has(key));
const missingInEn = [...beKeys].filter(key => !enKeys.has(key));

if (missingInBe.length || missingInEn.length) {
  console.error(JSON.stringify({ missingInBe, missingInEn }, null, 2));
  process.exit(1);
}
console.log(`Translation parity OK: ${enKeys.size} keys in en and be.`);
