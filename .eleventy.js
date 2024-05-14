const markdownIt = require("markdown-it");
const sass = require("sass");
let mila = require("markdown-it-link-attributes")

module.exports = function (eleventyConfig) {

  let milaOptions = {
    matcher(href, config) {
      return href.startsWith("http");
    },
    attrs: {
      target: '_blank',
      rel: 'noopener noreferrer',
    }
  };

  const md = new markdownIt({ 
    html: true,
    linkify: true,
    breaks: true,
    typographer: true
  }).use(mila, milaOptions);
  eleventyConfig.addFilter("markdown", (content) => {
    if (!content) return '';
    return md.render(content);
  });

  /** Setup SASS */
  eleventyConfig.addTemplateFormats("scss");
  eleventyConfig.addExtension("scss", {
    outputFileExtension: "css", // optional, default: "html"
    // `compile` is called once per .scss file in the input directory
    compile: async function(inputContent) {
      let result = sass.compileString(inputContent);
      // This is the render function, `data` is the full data cascade
      return async (data) => { return result.css; };
    }
  });

  /** Server Setup */
  eleventyConfig.setBrowserSyncConfig({
    server: {
      baseDir: `public`,
      middleware: [
        function (req, res, next) {
          let file = url.parse(req.url);
          file = file.pathname;
          file = file.replace(/\/+$/, ''); // remove trailing hash
          file = `public/${file}.html`;
  
          if (fs.existsSync(file)) {
            const content = fs.readFileSync(file);
            res.write(content);
            res.writeHead(200);
            res.end();
          } else {
            return next();
          }
        },
      ],
    },
    callbacks: {
      ready: function (err, bs) {

        bs.addMiddleware("*", (req, res) => {
          const content_404 = fs.readFileSync('404.html');
          // Add 404 http status code in request header.
          res.writeHead(404, { "Content-Type": "text/html; charset=UTF-8" });
          // Provides the 404 content without redirect.
          res.write(content_404);
          res.end();
        });
      }
    }
  });

    // Copy over
    eleventyConfig.addPassthroughCopy("img");
    eleventyConfig.addPassthroughCopy("scripts");

};