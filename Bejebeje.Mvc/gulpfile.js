/// <binding Clean='clean' />
"use strict";

const { src, dest, parallel, series } = require("gulp");
const rimraf = require("rimraf");
const concat = require("gulp-concat");
const cssnano = require('gulp-cssnano');
const uglify = require("gulp-uglify");
const sass = require("gulp-sass")(require("sass"));
const rename = require("gulp-rename");

let paths = {
  webroot: "./wwwroot/"
};

paths.sass = paths.webroot + "scss/**/*.scss";
paths.css = paths.webroot + "css/**/*.css";
paths.minCss = paths.webroot + "css/**/*.min.css";
paths.finalCssDestination = paths.webroot + "css/styles.min.css";

// delete existing css
function clearCssFiles(cb) {
  rimraf(paths.css, cb);
}

// convert sass to css
function convertSassToCss() {
  return (
    src(paths.sass)
      .pipe(sass.sync().on('error', sass.logError))
      .pipe(dest("./wwwroot/css/"))
  );
}

// minify the css
function minifyCss() {
  return (
    src(paths.webroot + "css/styles.css")
      .pipe(cssnano())
      .pipe(dest(paths.webroot + "/css/")));
}

exports.default = series(clearCssFiles, convertSassToCss, minifyCss);
