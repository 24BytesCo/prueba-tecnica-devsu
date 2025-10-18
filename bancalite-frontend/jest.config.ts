import type { Config } from 'jest';

const config: Config = {
  preset: 'jest-preset-angular',
  testEnvironment: 'jsdom',
  setupFilesAfterEnv: ['<rootDir>/setup-jest.ts'],
  testMatch: ['<rootDir>/src/**/*.spec.ts'],
  moduleFileExtensions: ['ts', 'html', 'js', 'json'],
  transform: {
    '^.+\\.(ts|mjs|html)$': 'jest-preset-angular'
  },
  transformIgnorePatterns: ['node_modules/(?!.*\\.mjs$)'],
  collectCoverageFrom: ['src/app/**/*.ts', '!src/app/**/*.spec.ts']
};

export default config;

