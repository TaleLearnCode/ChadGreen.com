// @ts-check
import { defineConfig } from 'astro/config';

// https://astro.build/config
export default defineConfig({
  site: 'https://chadgreen.com',
  output: 'static',
  build: {
    assets: '_astro'
  },
  vite: {
    build: {
      cssMinify: true
    }
  },
  markdown: {
    syntaxHighlight: {
      type: 'shiki',
    },
  },
});
