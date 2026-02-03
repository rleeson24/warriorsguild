#!/usr/bin/env node
"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (g && (g = 0, op[0] && (_ = 0)), _) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
Object.defineProperty(exports, "__esModule", { value: true });
/**
 * Concatenation + minification - replaces BuildBundlerMinifier.
 * Reads bundleconfig.json and produces bundles by concatenating files (no module resolution).
 * This matches BuildBundlerMinifier behavior: scripts expect globals (jQuery, ko, etc.) from separate script tags.
 * Run from WarriorsGuild directory. TypeScript must be compiled first (tsc or dotnet build).
 */
var esbuild = require("esbuild");
var fs = require("fs");
var path = require("path");
var url_1 = require("url");
var __dirname = path.dirname((0, url_1.fileURLToPath)(import.meta.url));
var projectRoot = path.resolve(__dirname, '..');
var bundleConfigPath = path.join(projectRoot, 'bundleconfig.json');
function resolveInputPath(relPath) {
    var fullPath = path.join(projectRoot, relPath.replace(/\//g, path.sep));
    if (fs.existsSync(fullPath))
        return fullPath;
    var alt = fullPath.replace(/Scripts([/\\])/gi, 'scripts$1');
    if (fs.existsSync(alt))
        return alt;
    var alt2 = fullPath.replace(/bootstrapAlert/g, 'BootstrapAlert');
    if (fs.existsSync(alt2))
        return alt2;
    var alt3 = fullPath.replace(/baseservice\.js$/i, 'BaseService.js');
    if (fs.existsSync(alt3))
        return alt3;
    return fullPath;
}
function build() {
    var _a;
    return __awaiter(this, void 0, void 0, function () {
        var content, config, bundlesDir, _i, config_1, bundle, outputFileName, inputFiles, minify, sourceMap, minifyEnabled, outPath, resolvedInputs, missing, ext, isCss, concatenated, output, result;
        return __generator(this, function (_b) {
            switch (_b.label) {
                case 0:
                    content = fs.readFileSync(bundleConfigPath, 'utf8');
                    if (content.charCodeAt(0) === 0xfeff)
                        content = content.slice(1);
                    config = JSON.parse(content);
                    bundlesDir = path.join(projectRoot, 'wwwroot', 'bundles');
                    if (!fs.existsSync(bundlesDir)) {
                        fs.mkdirSync(bundlesDir, { recursive: true });
                    }
                    _i = 0, config_1 = config;
                    _b.label = 1;
                case 1:
                    if (!(_i < config_1.length)) return [3 /*break*/, 5];
                    bundle = config_1[_i];
                    outputFileName = bundle.outputFileName, inputFiles = bundle.inputFiles, minify = bundle.minify, sourceMap = bundle.sourceMap;
                    minifyEnabled = (_a = minify === null || minify === void 0 ? void 0 : minify.enabled) !== null && _a !== void 0 ? _a : true;
                    outPath = path.join(projectRoot, outputFileName.replace(/\//g, path.sep));
                    resolvedInputs = inputFiles.map(function (f) { return resolveInputPath(f); });
                    missing = resolvedInputs.filter(function (p) { return !fs.existsSync(p); });
                    if (missing.length > 0) {
                        console.error("Missing inputs for ".concat(outputFileName, ":"));
                        missing.forEach(function (p) { return console.error("  - ".concat(path.relative(projectRoot, p))); });
                        process.exit(1);
                    }
                    ext = path.extname(outputFileName).toLowerCase();
                    isCss = ext === '.css';
                    concatenated = resolvedInputs
                        .map(function (p) { return fs.readFileSync(p, 'utf8'); })
                        .join('\n');
                    output = concatenated;
                    if (!minifyEnabled) return [3 /*break*/, 3];
                    return [4 /*yield*/, esbuild.transform(output, {
                            loader: isCss ? 'css' : 'js',
                            minify: true,
                        })];
                case 2:
                    result = _b.sent();
                    output = result.code;
                    _b.label = 3;
                case 3:
                    fs.writeFileSync(outPath, output, 'utf8');
                    console.log("  ".concat(path.basename(outputFileName)));
                    _b.label = 4;
                case 4:
                    _i++;
                    return [3 /*break*/, 1];
                case 5:
                    console.log("Bundled ".concat(config.length, " bundles."));
                    return [2 /*return*/];
            }
        });
    });
}
build().catch(function (err) {
    console.error(err);
    process.exit(1);
});
//# sourceMappingURL=esbuild-bundle.mjs.map