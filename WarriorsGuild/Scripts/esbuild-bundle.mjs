#!/usr/bin/env node
/**
 * Concatenation + minification - replaces BuildBundlerMinifier.
 * Reads bundleconfig.json and produces bundles by concatenating files (no module resolution).
 * This matches BuildBundlerMinifier behavior: scripts expect globals (jQuery, ko, etc.) from separate script tags.
 * Run from WarriorsGuild directory. TypeScript must be compiled first (tsc or dotnet build).
 */
import * as esbuild from 'esbuild';
import * as fs from 'fs';
import * as path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const projectRoot = path.resolve(__dirname, '..');
const bundleConfigPath = path.join(projectRoot, 'bundleconfig.json');

function resolveInputPath(relPath) {
  const fullPath = path.join(projectRoot, relPath.replace(/\//g, path.sep));
  if (fs.existsSync(fullPath)) return fullPath;
  const alt = fullPath.replace(/Scripts([/\\])/gi, 'scripts$1');
  if (fs.existsSync(alt)) return alt;
  const alt2 = fullPath.replace(/bootstrapAlert/g, 'BootstrapAlert');
  if (fs.existsSync(alt2)) return alt2;
  const alt3 = fullPath.replace(/baseservice\.js$/i, 'BaseService.js');
  if (fs.existsSync(alt3)) return alt3;
  return fullPath;
}

async function build() {
  let content = fs.readFileSync(bundleConfigPath, 'utf8');
  if (content.charCodeAt(0) === 0xfeff) content = content.slice(1);
  const config = JSON.parse(content);
  const bundlesDir = path.join(projectRoot, 'wwwroot', 'bundles');
  if (!fs.existsSync(bundlesDir)) {
    fs.mkdirSync(bundlesDir, { recursive: true });
  }

  for (const bundle of config) {
    const { outputFileName, inputFiles, minify, sourceMap } = bundle;
    const minifyEnabled = minify?.enabled ?? true;
    const outPath = path.join(projectRoot, outputFileName.replace(/\//g, path.sep));

    const resolvedInputs = inputFiles.map((f) => resolveInputPath(f));
    const missing = resolvedInputs.filter((p) => !fs.existsSync(p));
    if (missing.length > 0) {
      console.error(`Missing inputs for ${outputFileName}:`);
      missing.forEach((p) => console.error(`  - ${path.relative(projectRoot, p)}`));
      process.exit(1);
    }

    const ext = path.extname(outputFileName).toLowerCase();
    const isCss = ext === '.css';

    // Concatenate files in order (same as BuildBundlerMinifier - no module resolution)
    const concatenated = resolvedInputs
      .map((p) => fs.readFileSync(p, 'utf8'))
      .join('\n');

    let output = concatenated;
    if (minifyEnabled) {
      const result = await esbuild.transform(output, {
        loader: isCss ? 'css' : 'js',
        minify: true,
      });
      output = result.code;
    }

    fs.writeFileSync(outPath, output, 'utf8');
    console.log(`  ${path.basename(outputFileName)}`);
  }

  console.log(`Bundled ${config.length} bundles.`);
}

build().catch((err) => {
  console.error(err);
  process.exit(1);
});
