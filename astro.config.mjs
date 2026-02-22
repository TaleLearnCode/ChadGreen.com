// @ts-check
import { defineConfig } from 'astro/config';
import rehypeMermaid from 'rehype-mermaid';

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
      excludeLangs: ['mermaid'],
    },
    rehypePlugins: [rehypeMermaid],
  },
});
