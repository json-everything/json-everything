//! Licensed to the .NET Foundation under one or more agreements.
//! The .NET Foundation licenses this file to you under the MIT license.

var e=!1;const t=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,4,1,96,0,0,3,2,1,0,10,8,1,6,0,6,64,25,11,11])),o=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,5,1,96,0,1,123,3,2,1,0,10,15,1,13,0,65,1,253,15,65,2,253,15,253,128,2,11])),n=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,5,1,96,0,1,123,3,2,1,0,10,10,1,8,0,65,0,253,15,253,98,11])),r=Symbol.for("wasm promise_control");function i(e,t){let o=null;const n=new Promise((function(n,r){o={isDone:!1,promise:null,resolve:t=>{o.isDone||(o.isDone=!0,n(t),e&&e())},reject:e=>{o.isDone||(o.isDone=!0,r(e),t&&t())}}}));o.promise=n;const i=n;return i[r]=o,{promise:i,promise_control:o}}function s(e){return e[r]}function a(e){e&&function(e){return void 0!==e[r]}(e)||Be(!1,"Promise is not controllable")}const l="__mono_message__",c=["debug","log","trace","warn","info","error"],d="MONO_WASM: ";let u,f,m,g,p,h;function w(e){g=e}function b(e){if(Pe.diagnosticTracing){const t="function"==typeof e?e():e;console.debug(d+t)}}function y(e,...t){console.info(d+e,...t)}function v(e,...t){console.info(e,...t)}function E(e,...t){console.warn(d+e,...t)}function _(e,...t){if(t&&t.length>0&&t[0]&&"object"==typeof t[0]){if(t[0].silent)return;if(t[0].toString)return void console.error(d+e,t[0].toString())}console.error(d+e,...t)}function x(e,t,o){return function(...n){try{let r=n[0];if(void 0===r)r="undefined";else if(null===r)r="null";else if("function"==typeof r)r=r.toString();else if("string"!=typeof r)try{r=JSON.stringify(r)}catch(e){r=r.toString()}t(o?JSON.stringify({method:e,payload:r,arguments:n.slice(1)}):[e+r,...n.slice(1)])}catch(e){m.error(`proxyConsole failed: ${e}`)}}}function j(e,t,o){f=t,g=e,m={...t};const n=`${o}/console`.replace("https://","wss://").replace("http://","ws://");u=new WebSocket(n),u.addEventListener("error",A),u.addEventListener("close",S),function(){for(const e of c)f[e]=x(`console.${e}`,T,!0)}()}function R(e){let t=30;const o=()=>{u?0==u.bufferedAmount||0==t?(e&&v(e),function(){for(const e of c)f[e]=x(`console.${e}`,m.log,!1)}(),u.removeEventListener("error",A),u.removeEventListener("close",S),u.close(1e3,e),u=void 0):(t--,globalThis.setTimeout(o,100)):e&&m&&m.log(e)};o()}function T(e){u&&u.readyState===WebSocket.OPEN?u.send(e):m.log(e)}function A(e){m.error(`[${g}] proxy console websocket error: ${e}`,e)}function S(e){m.debug(`[${g}] proxy console websocket closed: ${e}`,e)}function D(){Pe.preferredIcuAsset=O(Pe.config);let e="invariant"==Pe.config.globalizationMode;if(!e)if(Pe.preferredIcuAsset)Pe.diagnosticTracing&&b("ICU data archive(s) available, disabling invariant mode");else{if("custom"===Pe.config.globalizationMode||"all"===Pe.config.globalizationMode||"sharded"===Pe.config.globalizationMode){const e="invariant globalization mode is inactive and no ICU data archives are available";throw _(`ERROR: ${e}`),new Error(e)}Pe.diagnosticTracing&&b("ICU data archive(s) not available, using invariant globalization mode"),e=!0,Pe.preferredIcuAsset=null}const t="DOTNET_SYSTEM_GLOBALIZATION_INVARIANT",o=Pe.config.environmentVariables;if(void 0===o[t]&&e&&(o[t]="1"),void 0===o.TZ)try{const e=Intl.DateTimeFormat().resolvedOptions().timeZone||null;e&&(o.TZ=e)}catch(e){y("failed to detect timezone, will fallback to UTC")}}function O(e){var t;if((null===(t=e.resources)||void 0===t?void 0:t.icu)&&"invariant"!=e.globalizationMode){const t=e.applicationCulture||(ke?globalThis.navigator&&globalThis.navigator.languages&&globalThis.navigator.languages[0]:Intl.DateTimeFormat().resolvedOptions().locale),o=e.resources.icu;let n=null;if("custom"===e.globalizationMode){if(o.length>=1)return o[0].name}else t&&"all"!==e.globalizationMode?"sharded"===e.globalizationMode&&(n=function(e){const t=e.split("-")[0];return"en"===t||["fr","fr-FR","it","it-IT","de","de-DE","es","es-ES"].includes(e)?"icudt_EFIGS.dat":["zh","ko","ja"].includes(t)?"icudt_CJK.dat":"icudt_no_CJK.dat"}(t)):n="icudt.dat";if(n)for(let e=0;e<o.length;e++){const t=o[e];if(t.virtualPath===n)return t.name}}return e.globalizationMode="invariant",null}(new Date).valueOf();const C=class{constructor(e){this.url=e}toString(){return this.url}};async function k(e,t){try{const o="function"==typeof globalThis.fetch;if(Se){const n=e.startsWith("file://");if(!n&&o)return globalThis.fetch(e,t||{credentials:"same-origin"});p||(h=Ne.require("url"),p=Ne.require("fs")),n&&(e=h.fileURLToPath(e));const r=await p.promises.readFile(e);return{ok:!0,headers:{length:0,get:()=>null},url:e,arrayBuffer:()=>r,json:()=>JSON.parse(r),text:()=>{throw new Error("NotImplementedException")}}}if(o)return globalThis.fetch(e,t||{credentials:"same-origin"});if("function"==typeof read)return{ok:!0,url:e,headers:{length:0,get:()=>null},arrayBuffer:()=>new Uint8Array(read(e,"binary")),json:()=>JSON.parse(read(e,"utf8")),text:()=>read(e,"utf8")}}catch(t){return{ok:!1,url:e,status:500,headers:{length:0,get:()=>null},statusText:"ERR28: "+t,arrayBuffer:()=>{throw t},json:()=>{throw t},text:()=>{throw t}}}throw new Error("No fetch implementation available")}function I(e){return"string"!=typeof e&&Be(!1,"url must be a string"),!M(e)&&0!==e.indexOf("./")&&0!==e.indexOf("../")&&globalThis.URL&&globalThis.document&&globalThis.document.baseURI&&(e=new URL(e,globalThis.document.baseURI).toString()),e}const U=/^[a-zA-Z][a-zA-Z\d+\-.]*?:\/\//,P=/[a-zA-Z]:[\\/]/;function M(e){return Se||Ie?e.startsWith("/")||e.startsWith("\\")||-1!==e.indexOf("///")||P.test(e):U.test(e)}let L,N=0;const $=[],z=[],W=new Map,F={"js-module-threads":!0,"js-module-runtime":!0,"js-module-dotnet":!0,"js-module-native":!0,"js-module-diagnostics":!0},B={...F,"js-module-library-initializer":!0},V={...F,dotnetwasm:!0,heap:!0,manifest:!0},q={...B,manifest:!0},H={...B,dotnetwasm:!0},J={dotnetwasm:!0,symbols:!0},Z={...B,dotnetwasm:!0,symbols:!0},Q={symbols:!0};function G(e){return!("icu"==e.behavior&&e.name!=Pe.preferredIcuAsset)}function K(e,t,o){null!=t||(t=[]),Be(1==t.length,`Expect to have one ${o} asset in resources`);const n=t[0];return n.behavior=o,X(n),e.push(n),n}function X(e){V[e.behavior]&&W.set(e.behavior,e)}function Y(e){Be(V[e],`Unknown single asset behavior ${e}`);const t=W.get(e);if(t&&!t.resolvedUrl)if(t.resolvedUrl=Pe.locateFile(t.name),F[t.behavior]){const e=ge(t);e?("string"!=typeof e&&Be(!1,"loadBootResource response for 'dotnetjs' type should be a URL string"),t.resolvedUrl=e):t.resolvedUrl=ce(t.resolvedUrl,t.behavior)}else if("dotnetwasm"!==t.behavior)throw new Error(`Unknown single asset behavior ${e}`);return t}function ee(e){const t=Y(e);return Be(t,`Single asset for ${e} not found`),t}let te=!1;async function oe(){if(!te){te=!0,Pe.diagnosticTracing&&b("mono_download_assets");try{const e=[],t=[],o=(e,t)=>{!Z[e.behavior]&&G(e)&&Pe.expected_instantiated_assets_count++,!H[e.behavior]&&G(e)&&(Pe.expected_downloaded_assets_count++,t.push(se(e)))};for(const t of $)o(t,e);for(const e of z)o(e,t);Pe.allDownloadsQueued.promise_control.resolve(),Promise.all([...e,...t]).then((()=>{Pe.allDownloadsFinished.promise_control.resolve()})).catch((e=>{throw Pe.err("Error in mono_download_assets: "+e),Xe(1,e),e})),await Pe.runtimeModuleLoaded.promise;const n=async e=>{const t=await e;if(t.buffer){if(!Z[t.behavior]){t.buffer&&"object"==typeof t.buffer||Be(!1,"asset buffer must be array-like or buffer-like or promise of these"),"string"!=typeof t.resolvedUrl&&Be(!1,"resolvedUrl must be string");const e=t.resolvedUrl,o=await t.buffer,n=new Uint8Array(o);pe(t),await Ue.beforeOnRuntimeInitialized.promise,Ue.instantiate_asset(t,e,n)}}else J[t.behavior]?("symbols"===t.behavior&&(await Ue.instantiate_symbols_asset(t),pe(t)),J[t.behavior]&&++Pe.actual_downloaded_assets_count):(t.isOptional||Be(!1,"Expected asset to have the downloaded buffer"),!H[t.behavior]&&G(t)&&Pe.expected_downloaded_assets_count--,!Z[t.behavior]&&G(t)&&Pe.expected_instantiated_assets_count--)},r=[],i=[];for(const t of e)r.push(n(t));for(const e of t)i.push(n(e));Promise.all(r).then((()=>{Ce||Ue.coreAssetsInMemory.promise_control.resolve()})).catch((e=>{throw Pe.err("Error in mono_download_assets: "+e),Xe(1,e),e})),Promise.all(i).then((async()=>{Ce||(await Ue.coreAssetsInMemory.promise,Ue.allAssetsInMemory.promise_control.resolve())})).catch((e=>{throw Pe.err("Error in mono_download_assets: "+e),Xe(1,e),e}))}catch(e){throw Pe.err("Error in mono_download_assets: "+e),e}}}let ne=!1;function re(){if(ne)return;ne=!0;const e=Pe.config,t=[];if(e.assets)for(const t of e.assets)"object"!=typeof t&&Be(!1,`asset must be object, it was ${typeof t} : ${t}`),"string"!=typeof t.behavior&&Be(!1,"asset behavior must be known string"),"string"!=typeof t.name&&Be(!1,"asset name must be string"),t.resolvedUrl&&"string"!=typeof t.resolvedUrl&&Be(!1,"asset resolvedUrl could be string"),t.hash&&"string"!=typeof t.hash&&Be(!1,"asset resolvedUrl could be string"),t.pendingDownload&&"object"!=typeof t.pendingDownload&&Be(!1,"asset pendingDownload could be object"),t.isCore?$.push(t):z.push(t),X(t);else if(e.resources){const o=e.resources;o.wasmNative||Be(!1,"resources.wasmNative must be defined"),o.jsModuleNative||Be(!1,"resources.jsModuleNative must be defined"),o.jsModuleRuntime||Be(!1,"resources.jsModuleRuntime must be defined"),K(z,o.wasmNative,"dotnetwasm"),K(t,o.jsModuleNative,"js-module-native"),K(t,o.jsModuleRuntime,"js-module-runtime"),o.jsModuleDiagnostics&&K(t,o.jsModuleDiagnostics,"js-module-diagnostics");const n=(e,t,o)=>{const n=e;n.behavior=t,o?(n.isCore=!0,$.push(n)):z.push(n)};if(o.coreAssembly)for(let e=0;e<o.coreAssembly.length;e++)n(o.coreAssembly[e],"assembly",!0);if(o.assembly)for(let e=0;e<o.assembly.length;e++)n(o.assembly[e],"assembly",!o.coreAssembly);if(0!=e.debugLevel&&Pe.isDebuggingSupported()){if(o.corePdb)for(let e=0;e<o.corePdb.length;e++)n(o.corePdb[e],"pdb",!0);if(o.pdb)for(let e=0;e<o.pdb.length;e++)n(o.pdb[e],"pdb",!o.corePdb)}if(e.loadAllSatelliteResources&&o.satelliteResources)for(const e in o.satelliteResources)for(let t=0;t<o.satelliteResources[e].length;t++){const r=o.satelliteResources[e][t];r.culture=e,n(r,"resource",!o.coreAssembly)}if(o.coreVfs)for(let e=0;e<o.coreVfs.length;e++)n(o.coreVfs[e],"vfs",!0);if(o.vfs)for(let e=0;e<o.vfs.length;e++)n(o.vfs[e],"vfs",!o.coreVfs);const r=O(e);if(r&&o.icu)for(let e=0;e<o.icu.length;e++){const t=o.icu[e];t.name===r&&n(t,"icu",!1)}if(o.wasmSymbols)for(let e=0;e<o.wasmSymbols.length;e++)n(o.wasmSymbols[e],"symbols",!1)}if(e.appsettings)for(let t=0;t<e.appsettings.length;t++){const o=e.appsettings[t],n=he(o);"appsettings.json"!==n&&n!==`appsettings.${e.applicationEnvironment}.json`||z.push({name:o,behavior:"vfs",cache:"no-cache",useCredentials:!0})}e.assets=[...$,...z,...t]}async function ie(e){const t=await se(e);return await t.pendingDownloadInternal.response,t.buffer}async function se(e){try{return await ae(e)}catch(t){if(!Pe.enableDownloadRetry)throw t;if(Ie||Se)throw t;if(e.pendingDownload&&e.pendingDownloadInternal==e.pendingDownload)throw t;if(e.resolvedUrl&&-1!=e.resolvedUrl.indexOf("file://"))throw t;if(t&&404==t.status)throw t;e.pendingDownloadInternal=void 0,await Pe.allDownloadsQueued.promise;try{return Pe.diagnosticTracing&&b(`Retrying download '${e.name}'`),await ae(e)}catch(t){return e.pendingDownloadInternal=void 0,await new Promise((e=>globalThis.setTimeout(e,100))),Pe.diagnosticTracing&&b(`Retrying download (2) '${e.name}' after delay`),await ae(e)}}}async function ae(e){for(;L;)await L.promise;try{++N,N==Pe.maxParallelDownloads&&(Pe.diagnosticTracing&&b("Throttling further parallel downloads"),L=i());const t=await async function(e){if(e.pendingDownload&&(e.pendingDownloadInternal=e.pendingDownload),e.pendingDownloadInternal&&e.pendingDownloadInternal.response)return e.pendingDownloadInternal.response;if(e.buffer){const t=await e.buffer;return e.resolvedUrl||(e.resolvedUrl="undefined://"+e.name),e.pendingDownloadInternal={url:e.resolvedUrl,name:e.name,response:Promise.resolve({ok:!0,arrayBuffer:()=>t,json:()=>JSON.parse(new TextDecoder("utf-8").decode(t)),text:()=>{throw new Error("NotImplementedException")},headers:{get:()=>{}}})},e.pendingDownloadInternal.response}const t=e.loadRemote&&Pe.config.remoteSources?Pe.config.remoteSources:[""];let o;for(let n of t){n=n.trim(),"./"===n&&(n="");const t=le(e,n);e.name===t?Pe.diagnosticTracing&&b(`Attempting to download '${t}'`):Pe.diagnosticTracing&&b(`Attempting to download '${t}' for ${e.name}`);try{e.resolvedUrl=t;const n=fe(e);if(e.pendingDownloadInternal=n,o=await n.response,!o||!o.ok)continue;return o}catch(e){o||(o={ok:!1,url:t,status:0,statusText:""+e});continue}}const n=e.isOptional||e.name.match(/\.pdb$/)&&Pe.config.ignorePdbLoadErrors;if(o||Be(!1,`Response undefined ${e.name}`),!n){const t=new Error(`download '${o.url}' for ${e.name} failed ${o.status} ${o.statusText}`);throw t.status=o.status,t}y(`optional download '${o.url}' for ${e.name} failed ${o.status} ${o.statusText}`)}(e);return t?(J[e.behavior]||(e.buffer=await t.arrayBuffer(),++Pe.actual_downloaded_assets_count),e):e}finally{if(--N,L&&N==Pe.maxParallelDownloads-1){Pe.diagnosticTracing&&b("Resuming more parallel downloads");const e=L;L=void 0,e.promise_control.resolve()}}}function le(e,t){let o;return null==t&&Be(!1,`sourcePrefix must be provided for ${e.name}`),e.resolvedUrl?o=e.resolvedUrl:(o=""===t?"assembly"===e.behavior||"pdb"===e.behavior?e.name:"resource"===e.behavior&&e.culture&&""!==e.culture?`${e.culture}/${e.name}`:e.name:t+e.name,o=ce(Pe.locateFile(o),e.behavior)),o&&"string"==typeof o||Be(!1,"attemptUrl need to be path or url string"),o}function ce(e,t){return Pe.modulesUniqueQuery&&q[t]&&(e+=Pe.modulesUniqueQuery),e}let de=0;const ue=new Set;function fe(e){try{e.resolvedUrl||Be(!1,"Request's resolvedUrl must be set");const t=function(e){let t=e.resolvedUrl;if(Pe.loadBootResource){const o=ge(e);if(o instanceof Promise)return o;"string"==typeof o&&(t=o)}const o={};return e.cache?o.cache=e.cache:Pe.config.disableNoCacheFetch||(o.cache="no-cache"),e.useCredentials?o.credentials="include":!Pe.config.disableIntegrityCheck&&e.hash&&(o.integrity=e.hash),Pe.fetch_like(t,o)}(e),o={name:e.name,url:e.resolvedUrl,response:t};return ue.add(e.name),o.response.then((()=>{"assembly"==e.behavior&&Pe.loadedAssemblies.push(e.name),de++,Pe.onDownloadResourceProgress&&Pe.onDownloadResourceProgress(de,ue.size)})),o}catch(t){const o={ok:!1,url:e.resolvedUrl,status:500,statusText:"ERR29: "+t,arrayBuffer:()=>{throw t},json:()=>{throw t}};return{name:e.name,url:e.resolvedUrl,response:Promise.resolve(o)}}}const me={resource:"assembly",assembly:"assembly",pdb:"pdb",icu:"globalization",vfs:"configuration",manifest:"manifest",dotnetwasm:"dotnetwasm","js-module-dotnet":"dotnetjs","js-module-native":"dotnetjs","js-module-runtime":"dotnetjs","js-module-threads":"dotnetjs"};function ge(e){var t;if(Pe.loadBootResource){const o=null!==(t=e.hash)&&void 0!==t?t:"",n=e.resolvedUrl,r=me[e.behavior];if(r){const t=Pe.loadBootResource(r,e.name,n,o,e.behavior);return"string"==typeof t?I(t):t}}}function pe(e){e.pendingDownloadInternal=null,e.pendingDownload=null,e.buffer=null,e.moduleExports=null}function he(e){let t=e.lastIndexOf("/");return t>=0&&t++,e.substring(t)}async function we(e){e&&await Promise.all((null!=e?e:[]).map((e=>async function(e){try{const t=e.name;if(!e.moduleExports){const o=ce(Pe.locateFile(t),"js-module-library-initializer");Pe.diagnosticTracing&&b(`Attempting to import '${o}' for ${e}`),e.moduleExports=await import(/*! webpackIgnore: true */o)}Pe.libraryInitializers.push({scriptName:t,exports:e.moduleExports})}catch(t){E(`Failed to import library initializer '${e}': ${t}`)}}(e))))}async function be(e,t){if(!Pe.libraryInitializers)return;const o=[];for(let n=0;n<Pe.libraryInitializers.length;n++){const r=Pe.libraryInitializers[n];r.exports[e]&&o.push(ye(r.scriptName,e,(()=>r.exports[e](...t))))}await Promise.all(o)}async function ye(e,t,o){try{await o()}catch(o){throw E(`Failed to invoke '${t}' on library initializer '${e}': ${o}`),Xe(1,o),o}}function ve(e,t){if(e===t)return e;const o={...t};return void 0!==o.assets&&o.assets!==e.assets&&(o.assets=[...e.assets||[],...o.assets||[]]),void 0!==o.resources&&(o.resources=_e(e.resources||{assembly:[],jsModuleNative:[],jsModuleRuntime:[],wasmNative:[]},o.resources)),void 0!==o.environmentVariables&&(o.environmentVariables={...e.environmentVariables||{},...o.environmentVariables||{}}),void 0!==o.runtimeOptions&&o.runtimeOptions!==e.runtimeOptions&&(o.runtimeOptions=[...e.runtimeOptions||[],...o.runtimeOptions||[]]),Object.assign(e,o)}function Ee(e,t){if(e===t)return e;const o={...t};return o.config&&(e.config||(e.config={}),o.config=ve(e.config,o.config)),Object.assign(e,o)}function _e(e,t){if(e===t)return e;const o={...t};return void 0!==o.coreAssembly&&(o.coreAssembly=[...e.coreAssembly||[],...o.coreAssembly||[]]),void 0!==o.assembly&&(o.assembly=[...e.assembly||[],...o.assembly||[]]),void 0!==o.lazyAssembly&&(o.lazyAssembly=[...e.lazyAssembly||[],...o.lazyAssembly||[]]),void 0!==o.corePdb&&(o.corePdb=[...e.corePdb||[],...o.corePdb||[]]),void 0!==o.pdb&&(o.pdb=[...e.pdb||[],...o.pdb||[]]),void 0!==o.jsModuleWorker&&(o.jsModuleWorker=[...e.jsModuleWorker||[],...o.jsModuleWorker||[]]),void 0!==o.jsModuleNative&&(o.jsModuleNative=[...e.jsModuleNative||[],...o.jsModuleNative||[]]),void 0!==o.jsModuleDiagnostics&&(o.jsModuleDiagnostics=[...e.jsModuleDiagnostics||[],...o.jsModuleDiagnostics||[]]),void 0!==o.jsModuleRuntime&&(o.jsModuleRuntime=[...e.jsModuleRuntime||[],...o.jsModuleRuntime||[]]),void 0!==o.wasmSymbols&&(o.wasmSymbols=[...e.wasmSymbols||[],...o.wasmSymbols||[]]),void 0!==o.wasmNative&&(o.wasmNative=[...e.wasmNative||[],...o.wasmNative||[]]),void 0!==o.icu&&(o.icu=[...e.icu||[],...o.icu||[]]),void 0!==o.satelliteResources&&(o.satelliteResources=function(e,t){if(e===t)return e;for(const o in t)e[o]=[...e[o]||[],...t[o]||[]];return e}(e.satelliteResources||{},o.satelliteResources||{})),void 0!==o.modulesAfterConfigLoaded&&(o.modulesAfterConfigLoaded=[...e.modulesAfterConfigLoaded||[],...o.modulesAfterConfigLoaded||[]]),void 0!==o.modulesAfterRuntimeReady&&(o.modulesAfterRuntimeReady=[...e.modulesAfterRuntimeReady||[],...o.modulesAfterRuntimeReady||[]]),void 0!==o.extensions&&(o.extensions={...e.extensions||{},...o.extensions||{}}),void 0!==o.vfs&&(o.vfs=[...e.vfs||[],...o.vfs||[]]),Object.assign(e,o)}function xe(){const e=Pe.config;if(e.environmentVariables=e.environmentVariables||{},e.runtimeOptions=e.runtimeOptions||[],e.resources=e.resources||{assembly:[],jsModuleNative:[],jsModuleWorker:[],jsModuleRuntime:[],wasmNative:[],vfs:[],satelliteResources:{}},e.assets){Pe.diagnosticTracing&&b("config.assets is deprecated, use config.resources instead");for(const t of e.assets){const o={};switch(t.behavior){case"assembly":o.assembly=[t];break;case"pdb":o.pdb=[t];break;case"resource":o.satelliteResources={},o.satelliteResources[t.culture]=[t];break;case"icu":o.icu=[t];break;case"symbols":o.wasmSymbols=[t];break;case"vfs":o.vfs=[t];break;case"dotnetwasm":o.wasmNative=[t];break;case"js-module-threads":o.jsModuleWorker=[t];break;case"js-module-runtime":o.jsModuleRuntime=[t];break;case"js-module-native":o.jsModuleNative=[t];break;case"js-module-diagnostics":o.jsModuleDiagnostics=[t];break;case"js-module-dotnet":break;default:throw new Error(`Unexpected behavior ${t.behavior} of asset ${t.name}`)}_e(e.resources,o)}}e.debugLevel,e.applicationEnvironment||(e.applicationEnvironment="Production"),e.applicationCulture&&(e.environmentVariables.LANG=`${e.applicationCulture}.UTF-8`),Ue.diagnosticTracing=Pe.diagnosticTracing=!!e.diagnosticTracing,Ue.waitForDebugger=e.waitForDebugger,Pe.maxParallelDownloads=e.maxParallelDownloads||Pe.maxParallelDownloads,Pe.enableDownloadRetry=void 0!==e.enableDownloadRetry?e.enableDownloadRetry:Pe.enableDownloadRetry}let je=!1;async function Re(e){var t;if(je)return void await Pe.afterConfigLoaded.promise;let o;try{if(e.configSrc||Pe.config&&0!==Object.keys(Pe.config).length&&(Pe.config.assets||Pe.config.resources)||(e.configSrc="dotnet.boot.js"),o=e.configSrc,je=!0,o&&(Pe.diagnosticTracing&&b("mono_wasm_load_config"),await async function(e){const t=e.configSrc,o=Pe.locateFile(t);let n=null;void 0!==Pe.loadBootResource&&(n=Pe.loadBootResource("manifest",t,o,"","manifest"));let r,i=null;if(n)if("string"==typeof n)n.includes(".json")?(i=await s(I(n)),r=await Ae(i)):r=(await import(I(n))).config;else{const e=await n;"function"==typeof e.json?(i=e,r=await Ae(i)):r=e.config}else o.includes(".json")?(i=await s(ce(o,"manifest")),r=await Ae(i)):r=(await import(ce(o,"manifest"))).config;function s(e){return Pe.fetch_like(e,{method:"GET",credentials:"include",cache:"no-cache"})}Pe.config.applicationEnvironment&&(r.applicationEnvironment=Pe.config.applicationEnvironment),ve(Pe.config,r)}(e)),xe(),await we(null===(t=Pe.config.resources)||void 0===t?void 0:t.modulesAfterConfigLoaded),await be("onRuntimeConfigLoaded",[Pe.config]),e.onConfigLoaded)try{await e.onConfigLoaded(Pe.config,Le),xe()}catch(e){throw _("onConfigLoaded() failed",e),e}xe(),Pe.afterConfigLoaded.promise_control.resolve(Pe.config)}catch(t){const n=`Failed to load config file ${o} ${t} ${null==t?void 0:t.stack}`;throw Pe.config=e.config=Object.assign(Pe.config,{message:n,error:t,isError:!0}),Xe(1,new Error(n)),t}}function Te(){return!!globalThis.navigator&&(Pe.isChromium||Pe.isFirefox)}async function Ae(e){const t=Pe.config,o=await e.json();t.applicationEnvironment||o.applicationEnvironment||(o.applicationEnvironment=e.headers.get("Blazor-Environment")||e.headers.get("DotNet-Environment")||void 0),o.environmentVariables||(o.environmentVariables={});const n=e.headers.get("DOTNET-MODIFIABLE-ASSEMBLIES");n&&(o.environmentVariables.DOTNET_MODIFIABLE_ASSEMBLIES=n);const r=e.headers.get("ASPNETCORE-BROWSER-TOOLS");return r&&(o.environmentVariables.__ASPNETCORE_BROWSER_TOOLS=r),o}"function"!=typeof importScripts||globalThis.onmessage||(globalThis.dotnetSidecar=!0);const Se="object"==typeof process&&"object"==typeof process.versions&&"string"==typeof process.versions.node,De="function"==typeof importScripts,Oe=De&&"undefined"!=typeof dotnetSidecar,Ce=De&&!Oe,ke="object"==typeof window||De&&!Se,Ie=!ke&&!Se;let Ue={},Pe={},Me={},Le={},Ne={},$e=!1;const ze={},We={config:ze},Fe={mono:{},binding:{},internal:Ne,module:We,loaderHelpers:Pe,runtimeHelpers:Ue,diagnosticHelpers:Me,api:Le};function Be(e,t){if(e)return;const o="Assert failed: "+("function"==typeof t?t():t),n=new Error(o);_(o,n),Ue.nativeAbort(n)}function Ve(){return void 0!==Pe.exitCode}function qe(){return Ue.runtimeReady&&!Ve()}function He(){Ve()&&Be(!1,`.NET runtime already exited with ${Pe.exitCode} ${Pe.exitReason}. You can use runtime.runMain() which doesn't exit the runtime.`),Ue.runtimeReady||Be(!1,".NET runtime didn't start yet. Please call dotnet.create() first.")}function Je(){ke&&(globalThis.addEventListener("unhandledrejection",et),globalThis.addEventListener("error",tt))}let Ze,Qe;function Ge(e){Qe&&Qe(e),Xe(e,Pe.exitReason)}function Ke(e){Ze&&Ze(e||Pe.exitReason),Xe(1,e||Pe.exitReason)}function Xe(t,o){var n,r;const i=o&&"object"==typeof o;t=i&&"number"==typeof o.status?o.status:void 0===t?-1:t;const s=i&&"string"==typeof o.message?o.message:""+o;(o=i?o:Ue.ExitStatus?function(e,t){const o=new Ue.ExitStatus(e);return o.message=t,o.toString=()=>t,o}(t,s):new Error("Exit with code "+t+" "+s)).status=t,o.message||(o.message=s);const a=""+(o.stack||(new Error).stack);try{Object.defineProperty(o,"stack",{get:()=>a})}catch(e){}const l=!!o.silent;if(o.silent=!0,Ve())Pe.diagnosticTracing&&b("mono_exit called after exit");else{try{We.onAbort==Ke&&(We.onAbort=Ze),We.onExit==Ge&&(We.onExit=Qe),ke&&(globalThis.removeEventListener("unhandledrejection",et),globalThis.removeEventListener("error",tt)),Ue.runtimeReady?(Ue.jiterpreter_dump_stats&&Ue.jiterpreter_dump_stats(!1),0===t&&(null===(n=Pe.config)||void 0===n?void 0:n.interopCleanupOnExit)&&Ue.forceDisposeProxies(!0,!0),e&&0!==t&&(null===(r=Pe.config)||void 0===r||r.dumpThreadsOnNonZeroExit)):(Pe.diagnosticTracing&&b(`abort_startup, reason: ${o}`),function(e){Pe.allDownloadsQueued.promise_control.reject(e),Pe.allDownloadsFinished.promise_control.reject(e),Pe.afterConfigLoaded.promise_control.reject(e),Pe.wasmCompilePromise.promise_control.reject(e),Pe.runtimeModuleLoaded.promise_control.reject(e),Ue.dotnetReady&&(Ue.dotnetReady.promise_control.reject(e),Ue.afterInstantiateWasm.promise_control.reject(e),Ue.beforePreInit.promise_control.reject(e),Ue.afterPreInit.promise_control.reject(e),Ue.afterPreRun.promise_control.reject(e),Ue.beforeOnRuntimeInitialized.promise_control.reject(e),Ue.afterOnRuntimeInitialized.promise_control.reject(e),Ue.afterPostRun.promise_control.reject(e))}(o))}catch(e){E("mono_exit A failed",e)}try{l||(function(e,t){if(0!==e&&t){const e=Ue.ExitStatus&&t instanceof Ue.ExitStatus?b:_;"string"==typeof t?e(t):(void 0===t.stack&&(t.stack=(new Error).stack+""),t.message?e(Ue.stringify_as_error_with_stack?Ue.stringify_as_error_with_stack(t.message+"\n"+t.stack):t.message+"\n"+t.stack):e(JSON.stringify(t)))}!Ce&&Pe.config&&(Pe.config.logExitCode?Pe.config.forwardConsoleLogsToWS?R("WASM EXIT "+e):v("WASM EXIT "+e):Pe.config.forwardConsoleLogsToWS&&R())}(t,o),function(e){if(ke&&!Ce&&Pe.config&&Pe.config.appendElementOnExit&&document){const t=document.createElement("label");t.id="tests_done",0!==e&&(t.style.background="red"),t.innerHTML=""+e,document.body.appendChild(t)}}(t))}catch(e){E("mono_exit B failed",e)}Pe.exitCode=t,Pe.exitReason||(Pe.exitReason=o),!Ce&&Ue.runtimeReady&&We.runtimeKeepalivePop()}if(Pe.config&&Pe.config.asyncFlushOnExit&&0===t)throw(async()=>{try{await async function(){try{const e=await import(/*! webpackIgnore: true */"process"),t=e=>new Promise(((t,o)=>{e.on("error",o),e.end("","utf8",t)})),o=t(e.stderr),n=t(e.stdout);let r;const i=new Promise((e=>{r=setTimeout((()=>e("timeout")),1e3)}));await Promise.race([Promise.all([n,o]),i]),clearTimeout(r)}catch(e){_(`flushing std* streams failed: ${e}`)}}()}finally{Ye(t,o)}})(),o;Ye(t,o)}function Ye(e,t){if(Ue.runtimeReady&&Ue.nativeExit)try{Ue.nativeExit(e)}catch(e){!Ue.ExitStatus||e instanceof Ue.ExitStatus||E("set_exit_code_and_quit_now failed: "+e.toString())}if(0!==e||!ke)throw Se&&Ne.process?Ne.process.exit(e):Ue.quit&&Ue.quit(e,t),t}function et(e){ot(e,e.reason,"rejection")}function tt(e){ot(e,e.error,"error")}function ot(e,t,o){e.preventDefault();try{t||(t=new Error("Unhandled "+o)),void 0===t.stack&&(t.stack=(new Error).stack),t.stack=t.stack+"",t.silent||(_("Unhandled error:",t),Xe(1,t))}catch(e){}}!function(e){if($e)throw new Error("Loader module already loaded");$e=!0,Ue=e.runtimeHelpers,Pe=e.loaderHelpers,Me=e.diagnosticHelpers,Le=e.api,Ne=e.internal,Object.assign(Le,{INTERNAL:Ne,invokeLibraryInitializers:be}),Object.assign(e.module,{config:ve(ze,{environmentVariables:{}})});const r={mono_wasm_bindings_is_ready:!1,config:e.module.config,diagnosticTracing:!1,nativeAbort:e=>{throw e||new Error("abort")},nativeExit:e=>{throw new Error("exit:"+e)}},l={gitHash:"c2435c3e0f46de784341ac3ed62863ce77e117b4",config:e.module.config,diagnosticTracing:!1,maxParallelDownloads:16,enableDownloadRetry:!0,_loaded_files:[],loadedFiles:[],loadedAssemblies:[],libraryInitializers:[],workerNextNumber:1,actual_downloaded_assets_count:0,actual_instantiated_assets_count:0,expected_downloaded_assets_count:0,expected_instantiated_assets_count:0,afterConfigLoaded:i(),allDownloadsQueued:i(),allDownloadsFinished:i(),wasmCompilePromise:i(),runtimeModuleLoaded:i(),loadingWorkers:i(),is_exited:Ve,is_runtime_running:qe,assert_runtime_running:He,mono_exit:Xe,createPromiseController:i,getPromiseController:s,assertIsControllablePromise:a,mono_download_assets:oe,resolve_single_asset_path:ee,setup_proxy_console:j,set_thread_prefix:w,installUnhandledErrorHandler:Je,retrieve_asset_download:ie,invokeLibraryInitializers:be,isDebuggingSupported:Te,exceptions:t,simd:n,relaxedSimd:o};Object.assign(Ue,r),Object.assign(Pe,l)}(Fe);let nt,rt,it,st=!1,at=!1;async function lt(e){if(!at){if(at=!0,ke&&Pe.config.forwardConsoleLogsToWS&&void 0!==globalThis.WebSocket&&j("main",globalThis.console,globalThis.location.origin),We||Be(!1,"Null moduleConfig"),Pe.config||Be(!1,"Null moduleConfig.config"),"function"==typeof e){const t=e(Fe.api);if(t.ready)throw new Error("Module.ready couldn't be redefined.");Object.assign(We,t),Ee(We,t)}else{if("object"!=typeof e)throw new Error("Can't use moduleFactory callback of createDotnetRuntime function.");Ee(We,e)}await async function(e){if(Se){const e=await import(/*! webpackIgnore: true */"process"),t=14;if(e.versions.node.split(".")[0]<t)throw new Error(`NodeJS at '${e.execPath}' has too low version '${e.versions.node}', please use at least ${t}. See also https://aka.ms/dotnet-wasm-features`)}const t=/*! webpackIgnore: true */import.meta.url,o=t.indexOf("?");var n;if(o>0&&(Pe.modulesUniqueQuery=t.substring(o)),Pe.scriptUrl=t.replace(/\\/g,"/").replace(/[?#].*/,""),Pe.scriptDirectory=(n=Pe.scriptUrl).slice(0,n.lastIndexOf("/"))+"/",Pe.locateFile=e=>"URL"in globalThis&&globalThis.URL!==C?new URL(e,Pe.scriptDirectory).toString():M(e)?e:Pe.scriptDirectory+e,Pe.fetch_like=k,Pe.out=console.log,Pe.err=console.error,Pe.onDownloadResourceProgress=e.onDownloadResourceProgress,ke&&globalThis.navigator){const e=globalThis.navigator,t=e.userAgentData&&e.userAgentData.brands;t&&t.length>0?Pe.isChromium=t.some((e=>"Google Chrome"===e.brand||"Microsoft Edge"===e.brand||"Chromium"===e.brand)):e.userAgent&&(Pe.isChromium=e.userAgent.includes("Chrome"),Pe.isFirefox=e.userAgent.includes("Firefox"))}Ne.require=Se?await import(/*! webpackIgnore: true */"module").then((e=>e.createRequire(/*! webpackIgnore: true */import.meta.url))):Promise.resolve((()=>{throw new Error("require not supported")})),void 0===globalThis.URL&&(globalThis.URL=C)}(We)}}async function ct(e){return await lt(e),Ze=We.onAbort,Qe=We.onExit,We.onAbort=Ke,We.onExit=Ge,We.ENVIRONMENT_IS_PTHREAD?async function(){(function(){const e=new MessageChannel,t=e.port1,o=e.port2;t.addEventListener("message",(e=>{var n,r;n=JSON.parse(e.data.config),r=JSON.parse(e.data.monoThreadInfo),st?Pe.diagnosticTracing&&b("mono config already received"):(ve(Pe.config,n),Ue.monoThreadInfo=r,xe(),Pe.diagnosticTracing&&b("mono config received"),st=!0,Pe.afterConfigLoaded.promise_control.resolve(Pe.config),ke&&n.forwardConsoleLogsToWS&&void 0!==globalThis.WebSocket&&Pe.setup_proxy_console("worker-idle",console,globalThis.location.origin)),t.close(),o.close()}),{once:!0}),t.start(),self.postMessage({[l]:{monoCmd:"preload",port:o}},[o])})(),await Pe.afterConfigLoaded.promise,function(){const e=Pe.config;e.assets||Be(!1,"config.assets must be defined");for(const t of e.assets)X(t),Q[t.behavior]&&z.push(t)}(),setTimeout((async()=>{try{await oe()}catch(e){Xe(1,e)}}),0);const e=dt(),t=await Promise.all(e);return await ut(t),We}():async function(){var e;await Re(We),re();const t=dt();(async function(){try{const e=ee("dotnetwasm");await se(e),e&&e.pendingDownloadInternal&&e.pendingDownloadInternal.response||Be(!1,"Can't load dotnet.native.wasm");const t=await e.pendingDownloadInternal.response,o=t.headers&&t.headers.get?t.headers.get("Content-Type"):void 0;let n;if("function"==typeof WebAssembly.compileStreaming&&"application/wasm"===o)n=await WebAssembly.compileStreaming(t);else{ke&&"application/wasm"!==o&&E('WebAssembly resource does not have the expected content type "application/wasm", so falling back to slower ArrayBuffer instantiation.');const e=await t.arrayBuffer();Pe.diagnosticTracing&&b("instantiate_wasm_module buffered"),n=Ie?await Promise.resolve(new WebAssembly.Module(e)):await WebAssembly.compile(e)}e.pendingDownloadInternal=null,e.pendingDownload=null,e.buffer=null,e.moduleExports=null,Pe.wasmCompilePromise.promise_control.resolve(n)}catch(e){Pe.wasmCompilePromise.promise_control.reject(e)}})(),setTimeout((async()=>{try{D(),await oe()}catch(e){Xe(1,e)}}),0);const o=await Promise.all(t);return await ut(o),await Ue.dotnetReady.promise,await we(null===(e=Pe.config.resources)||void 0===e?void 0:e.modulesAfterRuntimeReady),await be("onRuntimeReady",[Fe.api]),Le}()}function dt(){const e=ee("js-module-runtime"),t=ee("js-module-native");if(nt&&rt)return[nt,rt,it];"object"==typeof e.moduleExports?nt=e.moduleExports:(Pe.diagnosticTracing&&b(`Attempting to import '${e.resolvedUrl}' for ${e.name}`),nt=import(/*! webpackIgnore: true */e.resolvedUrl)),"object"==typeof t.moduleExports?rt=t.moduleExports:(Pe.diagnosticTracing&&b(`Attempting to import '${t.resolvedUrl}' for ${t.name}`),rt=import(/*! webpackIgnore: true */t.resolvedUrl));const o=Y("js-module-diagnostics");return o&&("object"==typeof o.moduleExports?it=o.moduleExports:(Pe.diagnosticTracing&&b(`Attempting to import '${o.resolvedUrl}' for ${o.name}`),it=import(/*! webpackIgnore: true */o.resolvedUrl))),[nt,rt,it]}async function ut(e){const{initializeExports:t,initializeReplacements:o,configureRuntimeStartup:n,configureEmscriptenStartup:r,configureWorkerStartup:i,setRuntimeGlobals:s,passEmscriptenInternals:a}=e[0],{default:l}=e[1],c=e[2];s(Fe),t(Fe),c&&c.setRuntimeGlobals(Fe),await n(We),Pe.runtimeModuleLoaded.promise_control.resolve(),l((e=>(Object.assign(We,{ready:e.ready,__dotnet_runtime:{initializeReplacements:o,configureEmscriptenStartup:r,configureWorkerStartup:i,passEmscriptenInternals:a}}),We))).catch((e=>{if(e.message&&e.message.toLowerCase().includes("out of memory"))throw new Error(".NET runtime has failed to start, because too much memory was requested. Please decrease the memory by adjusting EmccMaximumHeapSize. See also https://aka.ms/dotnet-wasm-features");throw e}))}const ft=new class{withModuleConfig(e){try{return Ee(We,e),this}catch(e){throw Xe(1,e),e}}withOnConfigLoaded(e){try{return Ee(We,{onConfigLoaded:e}),this}catch(e){throw Xe(1,e),e}}withConsoleForwarding(){try{return ve(ze,{forwardConsoleLogsToWS:!0}),this}catch(e){throw Xe(1,e),e}}withExitOnUnhandledError(){try{return ve(ze,{exitOnUnhandledError:!0}),Je(),this}catch(e){throw Xe(1,e),e}}withAsyncFlushOnExit(){try{return ve(ze,{asyncFlushOnExit:!0}),this}catch(e){throw Xe(1,e),e}}withExitCodeLogging(){try{return ve(ze,{logExitCode:!0}),this}catch(e){throw Xe(1,e),e}}withElementOnExit(){try{return ve(ze,{appendElementOnExit:!0}),this}catch(e){throw Xe(1,e),e}}withInteropCleanupOnExit(){try{return ve(ze,{interopCleanupOnExit:!0}),this}catch(e){throw Xe(1,e),e}}withDumpThreadsOnNonZeroExit(){try{return ve(ze,{dumpThreadsOnNonZeroExit:!0}),this}catch(e){throw Xe(1,e),e}}withWaitingForDebugger(e){try{return ve(ze,{waitForDebugger:e}),this}catch(e){throw Xe(1,e),e}}withInterpreterPgo(e,t){try{return ve(ze,{interpreterPgo:e,interpreterPgoSaveDelay:t}),ze.runtimeOptions?ze.runtimeOptions.push("--interp-pgo-recording"):ze.runtimeOptions=["--interp-pgo-recording"],this}catch(e){throw Xe(1,e),e}}withConfig(e){try{return ve(ze,e),this}catch(e){throw Xe(1,e),e}}withConfigSrc(e){try{return e&&"string"==typeof e||Be(!1,"must be file path or URL"),Ee(We,{configSrc:e}),this}catch(e){throw Xe(1,e),e}}withVirtualWorkingDirectory(e){try{return e&&"string"==typeof e||Be(!1,"must be directory path"),ve(ze,{virtualWorkingDirectory:e}),this}catch(e){throw Xe(1,e),e}}withEnvironmentVariable(e,t){try{const o={};return o[e]=t,ve(ze,{environmentVariables:o}),this}catch(e){throw Xe(1,e),e}}withEnvironmentVariables(e){try{return e&&"object"==typeof e||Be(!1,"must be dictionary object"),ve(ze,{environmentVariables:e}),this}catch(e){throw Xe(1,e),e}}withDiagnosticTracing(e){try{return"boolean"!=typeof e&&Be(!1,"must be boolean"),ve(ze,{diagnosticTracing:e}),this}catch(e){throw Xe(1,e),e}}withDebugging(e){try{return null!=e&&"number"==typeof e||Be(!1,"must be number"),ve(ze,{debugLevel:e}),this}catch(e){throw Xe(1,e),e}}withApplicationArguments(...e){try{return e&&Array.isArray(e)||Be(!1,"must be array of strings"),ve(ze,{applicationArguments:e}),this}catch(e){throw Xe(1,e),e}}withRuntimeOptions(e){try{return e&&Array.isArray(e)||Be(!1,"must be array of strings"),ze.runtimeOptions?ze.runtimeOptions.push(...e):ze.runtimeOptions=e,this}catch(e){throw Xe(1,e),e}}withMainAssembly(e){try{return ve(ze,{mainAssemblyName:e}),this}catch(e){throw Xe(1,e),e}}withApplicationArgumentsFromQuery(){try{if(!globalThis.window)throw new Error("Missing window to the query parameters from");if(void 0===globalThis.URLSearchParams)throw new Error("URLSearchParams is supported");const e=new URLSearchParams(globalThis.window.location.search).getAll("arg");return this.withApplicationArguments(...e)}catch(e){throw Xe(1,e),e}}withApplicationEnvironment(e){try{return ve(ze,{applicationEnvironment:e}),this}catch(e){throw Xe(1,e),e}}withApplicationCulture(e){try{return ve(ze,{applicationCulture:e}),this}catch(e){throw Xe(1,e),e}}withResourceLoader(e){try{return Pe.loadBootResource=e,this}catch(e){throw Xe(1,e),e}}async download(){try{await async function(){lt(We),await Re(We),re(),D(),oe(),await Pe.allDownloadsFinished.promise}()}catch(e){throw Xe(1,e),e}}async create(){try{return this.instance||(this.instance=await async function(){return await ct(We),Fe.api}()),this.instance}catch(e){throw Xe(1,e),e}}async run(){try{return We.config||Be(!1,"Null moduleConfig.config"),this.instance||await this.create(),this.instance.runMainAndExit()}catch(e){throw Xe(1,e),e}}},mt=Xe,gt=ct;Ie||"function"==typeof globalThis.URL||Be(!1,"This browser/engine doesn't support URL API. Please use a modern version. See also https://aka.ms/dotnet-wasm-features"),"function"!=typeof globalThis.BigInt64Array&&Be(!1,"This browser/engine doesn't support BigInt64Array API. Please use a modern version. See also https://aka.ms/dotnet-wasm-features"),ft.withConfig(/*json-start*/{
  "mainAssemblyName": "json-everything.net",
  "resources": {
    "hash": "sha256-WTZ7xw0Ad7bpHxCmlteOoxe2eS058ihCCzDrP4MX+k0=",
    "jsModuleNative": [
      {
        "name": "dotnet.native.js"
      }
    ],
    "jsModuleRuntime": [
      {
        "name": "dotnet.runtime.js"
      }
    ],
    "wasmNative": [
      {
        "name": "dotnet.native.wasm",
        "integrity": "sha256-tjie09uavTiX1TnkP9OL+OcnxCOs44ff5Dgws1rnAnc="
      }
    ],
    "icu": [
      {
        "virtualPath": "icudt_CJK.dat",
        "name": "icudt_CJK.dat",
        "integrity": "sha256-SZLtQnRc0JkwqHab0VUVP7T3uBPSeYzxzDnpxPpUnHk="
      },
      {
        "virtualPath": "icudt_EFIGS.dat",
        "name": "icudt_EFIGS.dat",
        "integrity": "sha256-8fItetYY8kQ0ww6oxwTLiT3oXlBwHKumbeP2pRF4yTc="
      },
      {
        "virtualPath": "icudt_no_CJK.dat",
        "name": "icudt_no_CJK.dat",
        "integrity": "sha256-L7sV7NEYP37/Qr2FPCePo5cJqRgTXRwGHuwF5Q+0Nfs="
      }
    ],
    "coreAssembly": [
      {
        "virtualPath": "System.Private.CoreLib.dll",
        "name": "System.Private.CoreLib.dll",
        "integrity": "sha256-YljiCZq6UfIwlIvHPfwmljQjgtTFNB1EvX3noT0ClIw="
      },
      {
        "virtualPath": "System.Runtime.InteropServices.JavaScript.dll",
        "name": "System.Runtime.InteropServices.JavaScript.dll",
        "integrity": "sha256-IRJ9oXnrKJflJEKZTANLwOmLYxZKHB5VuJ2auO2wGhg="
      }
    ],
    "assembly": [
      {
        "virtualPath": "BlazorMonaco.dll",
        "name": "BlazorMonaco.dll",
        "integrity": "sha256-0DwAMDShv0IXbPeW9PueG9CcLW9Q1GEhOIKnImY6joE="
      },
      {
        "virtualPath": "Blazored.LocalStorage.dll",
        "name": "Blazored.LocalStorage.dll",
        "integrity": "sha256-ZGOhicUlxsmaD44LUlk9QGefwrharIvp8bP1r4UKlPU="
      },
      {
        "virtualPath": "ColorCode.dll",
        "name": "ColorCode.dll",
        "integrity": "sha256-JGrcn2j0wtkL4DMqzxgENQkz77cEYJrRUUlofxUJL2Y="
      },
      {
        "virtualPath": "Humanizer.dll",
        "name": "Humanizer.dll",
        "integrity": "sha256-3qzxlUwzCke4pdc1Jx5XbepJN9HlvSoUXzrM+Gr+u84="
      },
      {
        "virtualPath": "IndexRange.dll",
        "name": "IndexRange.dll",
        "integrity": "sha256-E7RZqXJb865Csmg2YTIhRoRYlSv1n4SLgJwD7WAkf7E="
      },
      {
        "virtualPath": "Json.More.dll",
        "name": "Json.More.dll",
        "integrity": "sha256-tw8uMcmokX52fxq/Efn2v6K2seDNBMWvpiKmH9UWXLw="
      },
      {
        "virtualPath": "JsonE.Net.dll",
        "name": "JsonE.Net.dll",
        "integrity": "sha256-5ERCFViyosF1HcVF0fhlUSCtw32OB2JSHpMF1PJmGks="
      },
      {
        "virtualPath": "JsonLogic.dll",
        "name": "JsonLogic.dll",
        "integrity": "sha256-VJK956uh2aIEgmHPgj+kh7CUb+C0dB8IwLu6LCLEKnE="
      },
      {
        "virtualPath": "JsonPatch.Net.dll",
        "name": "JsonPatch.Net.dll",
        "integrity": "sha256-pXMbl4d77cSMxEgGsk0BzqmeECuyoWWdEoFrsYSLJPA="
      },
      {
        "virtualPath": "JsonPath.Net.dll",
        "name": "JsonPath.Net.dll",
        "integrity": "sha256-UwCa7LrCa4K5KEH2OGbuqG4auN6O8xIPMpy/UIjeWck="
      },
      {
        "virtualPath": "JsonPointer.Net.dll",
        "name": "JsonPointer.Net.dll",
        "integrity": "sha256-wF02CMs0ygGXLyJGP4aEoxPwTneJlsKZCF3SEXkVeBM="
      },
      {
        "virtualPath": "JsonSchema.Net.ArrayExt.dll",
        "name": "JsonSchema.Net.ArrayExt.dll",
        "integrity": "sha256-Iw4G7fFiWluqQ7rcZdxesmU18TiZ0n/eAyUql4BuG8k="
      },
      {
        "virtualPath": "JsonSchema.Net.Data.dll",
        "name": "JsonSchema.Net.Data.dll",
        "integrity": "sha256-V0jJD7UhHPadLMOtjRhpP2cXoyXmJInar2JEcA2PdPA="
      },
      {
        "virtualPath": "JsonSchema.Net.Generation.DataAnnotations.dll",
        "name": "JsonSchema.Net.Generation.DataAnnotations.dll",
        "integrity": "sha256-Wu5LkaeOp9bQ3u2TgE5TrGNI38dxZVVVaFylaOo40Os="
      },
      {
        "virtualPath": "JsonSchema.Net.Generation.dll",
        "name": "JsonSchema.Net.Generation.dll",
        "integrity": "sha256-F0qCwYz/pBolPJsO+L0ZrheY9WLs0JKkCq57MK6u7lk="
      },
      {
        "virtualPath": "JsonSchema.Net.OpenApi.dll",
        "name": "JsonSchema.Net.OpenApi.dll",
        "integrity": "sha256-psHI0zHliRgkH3z4X/TKtzdRVno+sQxddVYFtJMaYL4="
      },
      {
        "virtualPath": "JsonSchema.Net.dll",
        "name": "JsonSchema.Net.dll",
        "integrity": "sha256-K3RrrHBmavOiDLEQB0ed1oFJL1aV4Gx55WFqTtbzsdE="
      },
      {
        "virtualPath": "Markdig.SyntaxHighlighting.dll",
        "name": "Markdig.SyntaxHighlighting.dll",
        "integrity": "sha256-YEfO9OsS0T4kDjPXOW2pJF8qg62omn4UYMy/C2tcmC4="
      },
      {
        "virtualPath": "Markdig.dll",
        "name": "Markdig.dll",
        "integrity": "sha256-pPlLxfZAENt2DwY6/Bwa56mme7cU2W6rLidz/MJnDRg="
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Authorization.dll",
        "name": "Microsoft.AspNetCore.Authorization.dll",
        "integrity": "sha256-b8+75Y98hhW+hA8f8l2iG57hRdvA4XAJ6inES3KkPV0="
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.Forms.dll",
        "name": "Microsoft.AspNetCore.Components.Forms.dll",
        "integrity": "sha256-rTJOtjEibu9kJAtHrfRQeFczya+vHOeD6RKvFizRffw="
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.Web.dll",
        "name": "Microsoft.AspNetCore.Components.Web.dll",
        "integrity": "sha256-xuvgQWi/+VZLaRs0edPqSTxAP92DYbSF319rIc34GGU="
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.WebAssembly.dll",
        "name": "Microsoft.AspNetCore.Components.WebAssembly.dll",
        "integrity": "sha256-gxrzTZBxvEB7LHdiVAWmYp7viqr/0qFCavXYi/oTnW4="
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.dll",
        "name": "Microsoft.AspNetCore.Components.dll",
        "integrity": "sha256-MwSK06MZIUkMD+8eEZf7i+U5o3BVL24kbMi6mSDJT7g="
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Metadata.dll",
        "name": "Microsoft.AspNetCore.Metadata.dll",
        "integrity": "sha256-nTLGAPG1Sa+HKqEKV+MMKFxfYXmYJwD2bmoSHD+OrEY="
      },
      {
        "virtualPath": "Microsoft.Bcl.Memory.dll",
        "name": "Microsoft.Bcl.Memory.dll",
        "integrity": "sha256-I7bD2bICwqAThI130gTSDgM2BbtDzompv4830WlRGFs="
      },
      {
        "virtualPath": "Microsoft.CSharp.dll",
        "name": "Microsoft.CSharp.dll",
        "integrity": "sha256-S1BEhjsQnYOkzoA0TfnMy4TNC2LTx1+OvrumlMU9Vho="
      },
      {
        "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.dll",
        "name": "Microsoft.CodeAnalysis.CSharp.Scripting.dll",
        "integrity": "sha256-0PR5jenLeME0Y6Dz33R4FMF37BXgPhwDruFxNnmKLP4="
      },
      {
        "virtualPath": "Microsoft.CodeAnalysis.CSharp.dll",
        "name": "Microsoft.CodeAnalysis.CSharp.dll",
        "integrity": "sha256-z7XyeFwwSF7agD1Y+4Mw1zfLB9RpzT17wRE9WOjY5Ls="
      },
      {
        "virtualPath": "Microsoft.CodeAnalysis.Scripting.dll",
        "name": "Microsoft.CodeAnalysis.Scripting.dll",
        "integrity": "sha256-fDlzoJl6GQHE9ZeO+QHjrpbdt4DerXCkL2Xy4/byPo8="
      },
      {
        "virtualPath": "Microsoft.CodeAnalysis.dll",
        "name": "Microsoft.CodeAnalysis.dll",
        "integrity": "sha256-Cn+x2U4Of0Z5ixOMqmfl1fuPuY/TG1KUyRfe8FerPYA="
      },
      {
        "virtualPath": "Microsoft.DotNet.HotReload.WebAssembly.Browser.dll",
        "name": "Microsoft.DotNet.HotReload.WebAssembly.Browser.dll",
        "integrity": "sha256-nWw9Fs4NSzKWY/yRnoGWe6ppm8H5rAWT2gYn6ZFyhAc="
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.Abstractions.dll",
        "name": "Microsoft.Extensions.Configuration.Abstractions.dll",
        "integrity": "sha256-2iIDzYXnAVmCEJmeZxTkzIPSC4M7SvqSPjuEGW1vcqo="
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.Binder.dll",
        "name": "Microsoft.Extensions.Configuration.Binder.dll",
        "integrity": "sha256-8D7Y1OeJtGj4RksGmDqb69FWeV6wOio8TR48adv4QHo="
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.FileExtensions.dll",
        "name": "Microsoft.Extensions.Configuration.FileExtensions.dll",
        "integrity": "sha256-U7CNI/XRD3Rxge+meo98awzPjQZrpHYqrsuP1ABXU2I="
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.Json.dll",
        "name": "Microsoft.Extensions.Configuration.Json.dll",
        "integrity": "sha256-SHHuhAo7eCnkZJa764U1djkq9vRDOm57n+rChOTuyDw="
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.dll",
        "name": "Microsoft.Extensions.Configuration.dll",
        "integrity": "sha256-3041TEvL01qo7ajLDpw7qawXWen1bsUO7Q1mGIAwXkk="
      },
      {
        "virtualPath": "Microsoft.Extensions.DependencyInjection.Abstractions.dll",
        "name": "Microsoft.Extensions.DependencyInjection.Abstractions.dll",
        "integrity": "sha256-X9FkqDE7MlX+UadOXTD/QcXWv/jrOv9RVdLwNtfGbfQ="
      },
      {
        "virtualPath": "Microsoft.Extensions.DependencyInjection.dll",
        "name": "Microsoft.Extensions.DependencyInjection.dll",
        "integrity": "sha256-8MdXdo2+N/zYzbghcECq2zEkPwkr/L0xWKfhlmybDc0="
      },
      {
        "virtualPath": "Microsoft.Extensions.Diagnostics.Abstractions.dll",
        "name": "Microsoft.Extensions.Diagnostics.Abstractions.dll",
        "integrity": "sha256-TSDAFUBqe/GmR48bavVn2XL6K6RyfeYTT/LGQfkKs2w="
      },
      {
        "virtualPath": "Microsoft.Extensions.Diagnostics.dll",
        "name": "Microsoft.Extensions.Diagnostics.dll",
        "integrity": "sha256-A9Khk0b9LS4kEFSADtOO2HLLcJ0OhYChbxm7Z/37oUA="
      },
      {
        "virtualPath": "Microsoft.Extensions.FileProviders.Abstractions.dll",
        "name": "Microsoft.Extensions.FileProviders.Abstractions.dll",
        "integrity": "sha256-fuk1qt2K/i9zHWVe9UR57WIG5tKqUr/RiJzfFwsPk5k="
      },
      {
        "virtualPath": "Microsoft.Extensions.FileProviders.Physical.dll",
        "name": "Microsoft.Extensions.FileProviders.Physical.dll",
        "integrity": "sha256-IzSg4hQmgB/tUVLlWnMSotv8lL4G290q9EazSiLF7Po="
      },
      {
        "virtualPath": "Microsoft.Extensions.FileSystemGlobbing.dll",
        "name": "Microsoft.Extensions.FileSystemGlobbing.dll",
        "integrity": "sha256-XvxeiNGcpM238zR0g1kaXNAl1heEPt8rTt9XARArtr8="
      },
      {
        "virtualPath": "Microsoft.Extensions.Http.dll",
        "name": "Microsoft.Extensions.Http.dll",
        "integrity": "sha256-Rob65iJehWpCAYXHzJotHAyFdhH0yvfUF5avpuoY+c8="
      },
      {
        "virtualPath": "Microsoft.Extensions.Logging.Abstractions.dll",
        "name": "Microsoft.Extensions.Logging.Abstractions.dll",
        "integrity": "sha256-aRydsflgPAmxbD6ozU13Vv80ibMgnRYgNYRCbdYueCk="
      },
      {
        "virtualPath": "Microsoft.Extensions.Logging.dll",
        "name": "Microsoft.Extensions.Logging.dll",
        "integrity": "sha256-FbjvnL0JEy4b9k0uVCSwyfnLzbW8nb6lb8YHEkNBwyg="
      },
      {
        "virtualPath": "Microsoft.Extensions.Options.ConfigurationExtensions.dll",
        "name": "Microsoft.Extensions.Options.ConfigurationExtensions.dll",
        "integrity": "sha256-dALn12HtX6SJzvxpO6iWalz8Pq7zKEZYtpE8jmWAC64="
      },
      {
        "virtualPath": "Microsoft.Extensions.Options.dll",
        "name": "Microsoft.Extensions.Options.dll",
        "integrity": "sha256-HPKSc6RLQ06iByBsshfK2JbEjL9LvpRzg3hVNu6JQYs="
      },
      {
        "virtualPath": "Microsoft.Extensions.Primitives.dll",
        "name": "Microsoft.Extensions.Primitives.dll",
        "integrity": "sha256-MC0fHMRYag+WAWNSWV7V/+7rGOmepjv1So1f4ETJZC4="
      },
      {
        "virtualPath": "Microsoft.Extensions.Validation.dll",
        "name": "Microsoft.Extensions.Validation.dll",
        "integrity": "sha256-OhQpMTLw/l4TZxw991LVKDmySkJEhtN5OF1Xq9lrfzA="
      },
      {
        "virtualPath": "Microsoft.JSInterop.WebAssembly.dll",
        "name": "Microsoft.JSInterop.WebAssembly.dll",
        "integrity": "sha256-nvMmB7kWXhOtLE4XCn4jpn2th9qivwJ7GT7diT/Flwk="
      },
      {
        "virtualPath": "Microsoft.JSInterop.dll",
        "name": "Microsoft.JSInterop.dll",
        "integrity": "sha256-j2yE30gi6s9UwxjDxt2m1JOSFkXqAKe1ikbxybAi/nc="
      },
      {
        "virtualPath": "Microsoft.VisualBasic.Core.dll",
        "name": "Microsoft.VisualBasic.Core.dll",
        "integrity": "sha256-9SAWynyr+R5AuZ+NbZcZ7R7M+3ZV679AAMAwY2QpO5M="
      },
      {
        "virtualPath": "Microsoft.VisualBasic.dll",
        "name": "Microsoft.VisualBasic.dll",
        "integrity": "sha256-lKqoOE7xc5YKl34a3OmBSfDSEvfZk7F1eg1VznQOPbs="
      },
      {
        "virtualPath": "Microsoft.Win32.Primitives.dll",
        "name": "Microsoft.Win32.Primitives.dll",
        "integrity": "sha256-D+tcxk9D3N0wrPjA9RvxBIBUmRkv68tlpzSJs0gDsLA="
      },
      {
        "virtualPath": "Microsoft.Win32.Registry.dll",
        "name": "Microsoft.Win32.Registry.dll",
        "integrity": "sha256-yWaLK3iThPdkKFAgr4yLXCXK14c+KI0cwqKMki0uYfU="
      },
      {
        "virtualPath": "System.AppContext.dll",
        "name": "System.AppContext.dll",
        "integrity": "sha256-X3RpSDeDgq1thpvTDjYYJXU62l1u1Vs0+ej5N5xpnMM="
      },
      {
        "virtualPath": "System.Buffers.dll",
        "name": "System.Buffers.dll",
        "integrity": "sha256-6qKmeFCRtXT9yOt0iebqM/u+He8RtSi7RlGAOd4Kt8I="
      },
      {
        "virtualPath": "System.Collections.Concurrent.dll",
        "name": "System.Collections.Concurrent.dll",
        "integrity": "sha256-0TMR1RxRX2OsPPYXwUFw4oayK4++HJCwoQFHU8fT/es="
      },
      {
        "virtualPath": "System.Collections.Immutable.dll",
        "name": "System.Collections.Immutable.dll",
        "integrity": "sha256-qxoVQXZjY2bQxQC2EvWIs0f1lAlJS9XgtmC/oTQObgc="
      },
      {
        "virtualPath": "System.Collections.NonGeneric.dll",
        "name": "System.Collections.NonGeneric.dll",
        "integrity": "sha256-TdYSUIyOqv+Qbw5Up75xRqP5hnQ8yNf/58NxpuWdcgI="
      },
      {
        "virtualPath": "System.Collections.Specialized.dll",
        "name": "System.Collections.Specialized.dll",
        "integrity": "sha256-R2O+KSsVPd/emfSLzKSmAlNzfpUyfXojeQK+1YQwcsI="
      },
      {
        "virtualPath": "System.Collections.dll",
        "name": "System.Collections.dll",
        "integrity": "sha256-YxpTYei9Y7ze64oSuu43IXNiumC5Wcdhog82EKFPio4="
      },
      {
        "virtualPath": "System.ComponentModel.Annotations.dll",
        "name": "System.ComponentModel.Annotations.dll",
        "integrity": "sha256-4f5L6vWxBB02Ld+WCWGf73Z0/Gi3N1iOkY5AhG5Egxc="
      },
      {
        "virtualPath": "System.ComponentModel.DataAnnotations.dll",
        "name": "System.ComponentModel.DataAnnotations.dll",
        "integrity": "sha256-Nx11t/fm7fhtjiJYKHgckOhjYQRqNDDBUxfJMymUeRA="
      },
      {
        "virtualPath": "System.ComponentModel.EventBasedAsync.dll",
        "name": "System.ComponentModel.EventBasedAsync.dll",
        "integrity": "sha256-JtjNrDQAqJkXMM3uHrG/vijRvKjfkE+Z00asDvFUR4M="
      },
      {
        "virtualPath": "System.ComponentModel.Primitives.dll",
        "name": "System.ComponentModel.Primitives.dll",
        "integrity": "sha256-9h714tir39vsHYU04thpIl1wx+pKl6cex8HoCKmKtuM="
      },
      {
        "virtualPath": "System.ComponentModel.TypeConverter.dll",
        "name": "System.ComponentModel.TypeConverter.dll",
        "integrity": "sha256-ivrnK9pScf2t90kkmsQ7Y9sgfAh5O5OAqLYlEQP0vVw="
      },
      {
        "virtualPath": "System.ComponentModel.dll",
        "name": "System.ComponentModel.dll",
        "integrity": "sha256-e/WHZ8ORtud/bB7AyraUxHHeXCNx895J3w/b/HqDM6g="
      },
      {
        "virtualPath": "System.Configuration.dll",
        "name": "System.Configuration.dll",
        "integrity": "sha256-iXsN1mseZeqfqGF1H/leggZXG7rN0Zw12inRekeKTnc="
      },
      {
        "virtualPath": "System.Console.dll",
        "name": "System.Console.dll",
        "integrity": "sha256-g8deAvhVBy0RHqoda9KinYCkTI+JAtYC94XgHIAkpoM="
      },
      {
        "virtualPath": "System.Core.dll",
        "name": "System.Core.dll",
        "integrity": "sha256-5NTeM7Az/BLfwh4MXsdQEPC5dY32YgwwAj1NTGe3gqM="
      },
      {
        "virtualPath": "System.Data.Common.dll",
        "name": "System.Data.Common.dll",
        "integrity": "sha256-50O/emNukaeQUGR8z/Rr5sZL0ug9ygLFe1XmujJwkMk="
      },
      {
        "virtualPath": "System.Data.DataSetExtensions.dll",
        "name": "System.Data.DataSetExtensions.dll",
        "integrity": "sha256-GCus3VDOWjDasIhce9tBykSZZn026/Q1JVEjHfDSV+8="
      },
      {
        "virtualPath": "System.Data.dll",
        "name": "System.Data.dll",
        "integrity": "sha256-OjeUzfnB8uXPm67GtVruU+Fd7Gh8qJndJbfT6wBcErU="
      },
      {
        "virtualPath": "System.Diagnostics.Contracts.dll",
        "name": "System.Diagnostics.Contracts.dll",
        "integrity": "sha256-Nkc9ROlO8BQE8LRtzynw3okdtRGeO69Wp/tyj5v3Tis="
      },
      {
        "virtualPath": "System.Diagnostics.Debug.dll",
        "name": "System.Diagnostics.Debug.dll",
        "integrity": "sha256-i6Pg68773lMZ6+8lCRFxPJvB4s8NfD9ti1iG+10tKhs="
      },
      {
        "virtualPath": "System.Diagnostics.DiagnosticSource.dll",
        "name": "System.Diagnostics.DiagnosticSource.dll",
        "integrity": "sha256-F6QDHJsqeWmTq2KTUtbOFh2u8t9/SQixOvohUSSi8Rg="
      },
      {
        "virtualPath": "System.Diagnostics.FileVersionInfo.dll",
        "name": "System.Diagnostics.FileVersionInfo.dll",
        "integrity": "sha256-zPxFI9ayqzeknvnND2FvEn0sDyANJTuYQtzohOXxyKo="
      },
      {
        "virtualPath": "System.Diagnostics.Process.dll",
        "name": "System.Diagnostics.Process.dll",
        "integrity": "sha256-FgRCBVtBHhxzlXdVKtjYvJTO9C76ik+raRKzs3zGVdQ="
      },
      {
        "virtualPath": "System.Diagnostics.StackTrace.dll",
        "name": "System.Diagnostics.StackTrace.dll",
        "integrity": "sha256-ia3ClOCiZm+xBGc0eW12wkgYRvwn6JaIopYTMuFgMJw="
      },
      {
        "virtualPath": "System.Diagnostics.TextWriterTraceListener.dll",
        "name": "System.Diagnostics.TextWriterTraceListener.dll",
        "integrity": "sha256-6vCwGDD9BMB67cV43jh1545lmcnj6cgWWQVnQDvJAMc="
      },
      {
        "virtualPath": "System.Diagnostics.Tools.dll",
        "name": "System.Diagnostics.Tools.dll",
        "integrity": "sha256-O4FnD0paMwI0tSOWF8ufcSAGLX43q1GU9sr0G0eknsA="
      },
      {
        "virtualPath": "System.Diagnostics.TraceSource.dll",
        "name": "System.Diagnostics.TraceSource.dll",
        "integrity": "sha256-qptcvLuOTiqbY0Ih6eR5ZvEm9wHhvXcm+7bR/4DOXMU="
      },
      {
        "virtualPath": "System.Diagnostics.Tracing.dll",
        "name": "System.Diagnostics.Tracing.dll",
        "integrity": "sha256-5MESQnSsYCwLGW+A7QNzSrU6mzW/k75zrw+GxhwGRcE="
      },
      {
        "virtualPath": "System.Drawing.Primitives.dll",
        "name": "System.Drawing.Primitives.dll",
        "integrity": "sha256-awdwYOJbqL+JiLJt5rhroUIpxc1ZxvPa23d4K3Wd0xE="
      },
      {
        "virtualPath": "System.Drawing.dll",
        "name": "System.Drawing.dll",
        "integrity": "sha256-NZH0L7XloG6ASeq5VoNhJm4rGg14heiy6kzEpsj2NjY="
      },
      {
        "virtualPath": "System.Dynamic.Runtime.dll",
        "name": "System.Dynamic.Runtime.dll",
        "integrity": "sha256-RNNnq8c155dD4mjE9OklDLPznteyUOAtSZeSqqsSzII="
      },
      {
        "virtualPath": "System.Formats.Asn1.dll",
        "name": "System.Formats.Asn1.dll",
        "integrity": "sha256-x7vmPKM6upAvWP12bVsJ/cSmnEXAT8Sj5YkjDC4CmV0="
      },
      {
        "virtualPath": "System.Formats.Tar.dll",
        "name": "System.Formats.Tar.dll",
        "integrity": "sha256-NKuPbQC5xTon7mXpdGf8NOdTh0sFsfoBuZqs8yM+S8U="
      },
      {
        "virtualPath": "System.Globalization.Calendars.dll",
        "name": "System.Globalization.Calendars.dll",
        "integrity": "sha256-pvaRJnFR5WCGmN/Nlspn5LumfOao6cBUvXjUtvTGFuc="
      },
      {
        "virtualPath": "System.Globalization.Extensions.dll",
        "name": "System.Globalization.Extensions.dll",
        "integrity": "sha256-qqQoAONGtAwik9O9L2dC6f57JK2Zl0hb1NBgH11C7OE="
      },
      {
        "virtualPath": "System.Globalization.dll",
        "name": "System.Globalization.dll",
        "integrity": "sha256-0urvWg4jZUdNEX7gzwgm9lwNmI8fOoc6Z2kJ1OdW5tY="
      },
      {
        "virtualPath": "System.IO.Compression.Brotli.dll",
        "name": "System.IO.Compression.Brotli.dll",
        "integrity": "sha256-OttLGQI3v8eq9McEk5+0p3pe4rSeoSfz3Gnti+Muzag="
      },
      {
        "virtualPath": "System.IO.Compression.FileSystem.dll",
        "name": "System.IO.Compression.FileSystem.dll",
        "integrity": "sha256-taZG2PQ9vOVcB36kKR28MBlalQJDN5+iHFcTt9AutLA="
      },
      {
        "virtualPath": "System.IO.Compression.ZipFile.dll",
        "name": "System.IO.Compression.ZipFile.dll",
        "integrity": "sha256-QmgUQYzn9g6DgYdt7v1S60AMCapq76YTWw5n7Y2CkT4="
      },
      {
        "virtualPath": "System.IO.Compression.dll",
        "name": "System.IO.Compression.dll",
        "integrity": "sha256-09a8qGu46DxcLlZn5OGxFDhZCDClA7IGpPCZmDTs3CM="
      },
      {
        "virtualPath": "System.IO.FileSystem.AccessControl.dll",
        "name": "System.IO.FileSystem.AccessControl.dll",
        "integrity": "sha256-oIYjARuc8uc5yoHlKIyR5S/jDoYezxi0I9DK3k7MC0A="
      },
      {
        "virtualPath": "System.IO.FileSystem.DriveInfo.dll",
        "name": "System.IO.FileSystem.DriveInfo.dll",
        "integrity": "sha256-QVoM2Bcrj8s6T/uQceKCWGVdXbp1yRRW4gMBdw4/MJM="
      },
      {
        "virtualPath": "System.IO.FileSystem.Primitives.dll",
        "name": "System.IO.FileSystem.Primitives.dll",
        "integrity": "sha256-VUBHtkGAV3eObcM/ZMpD7+U91pQgZmG8gKQAWsHc9Jc="
      },
      {
        "virtualPath": "System.IO.FileSystem.Watcher.dll",
        "name": "System.IO.FileSystem.Watcher.dll",
        "integrity": "sha256-N90oLEKAOtzRXtAY34cYb/QqP2W1s8ARAlspC9ojvjw="
      },
      {
        "virtualPath": "System.IO.FileSystem.dll",
        "name": "System.IO.FileSystem.dll",
        "integrity": "sha256-x6MBUsrFQm6+uUj08cJRxl4+vj8no5fPnJcVLdXfipI="
      },
      {
        "virtualPath": "System.IO.IsolatedStorage.dll",
        "name": "System.IO.IsolatedStorage.dll",
        "integrity": "sha256-ZdaKEe7ZIcWNPrDaTcFXXkd1z6m9X2m70clBCpDDniY="
      },
      {
        "virtualPath": "System.IO.MemoryMappedFiles.dll",
        "name": "System.IO.MemoryMappedFiles.dll",
        "integrity": "sha256-5nofdztc585oMehx+jE1R+zeR1csocOjba29NZ6Ejok="
      },
      {
        "virtualPath": "System.IO.Pipelines.dll",
        "name": "System.IO.Pipelines.dll",
        "integrity": "sha256-x7CC/26hQ5oYRSfhKC246S9dVHnh+6MHRxAo184rinc="
      },
      {
        "virtualPath": "System.IO.Pipes.AccessControl.dll",
        "name": "System.IO.Pipes.AccessControl.dll",
        "integrity": "sha256-iPsoaBwxDfKNRnzQUg6aumG/MrZWszVrFFHvc8IwK/Y="
      },
      {
        "virtualPath": "System.IO.Pipes.dll",
        "name": "System.IO.Pipes.dll",
        "integrity": "sha256-gDj+aE5mJs5PP13lwoPKOlPUBmM/IeGGeAncXqrRK4k="
      },
      {
        "virtualPath": "System.IO.UnmanagedMemoryStream.dll",
        "name": "System.IO.UnmanagedMemoryStream.dll",
        "integrity": "sha256-dPiGk1+jJkxiFOizPUqfUP+gZJMa3FrMxfEDVuWcslI="
      },
      {
        "virtualPath": "System.IO.dll",
        "name": "System.IO.dll",
        "integrity": "sha256-Is2B29CVqh/379ICf777NLWXR5xlERT/tilD1nm1BLk="
      },
      {
        "virtualPath": "System.Linq.AsyncEnumerable.dll",
        "name": "System.Linq.AsyncEnumerable.dll",
        "integrity": "sha256-T+LOOSLtV3BAlNXNLTcN5DHG+n7KJEc4Ectw7nxv5S8="
      },
      {
        "virtualPath": "System.Linq.Expressions.dll",
        "name": "System.Linq.Expressions.dll",
        "integrity": "sha256-wpRygFbzL4zom0S039osSrCJGVns4FXTdEA8HDJ5DkI="
      },
      {
        "virtualPath": "System.Linq.Parallel.dll",
        "name": "System.Linq.Parallel.dll",
        "integrity": "sha256-uO77ndeNF6hoY7ICR/LnWf6T1/Qox7Cgn+TpPw4mm+M="
      },
      {
        "virtualPath": "System.Linq.Queryable.dll",
        "name": "System.Linq.Queryable.dll",
        "integrity": "sha256-cSvk9I9lldLIAzCdrVqBfnw4snYR0S9AV1nSAe5rDFE="
      },
      {
        "virtualPath": "System.Linq.dll",
        "name": "System.Linq.dll",
        "integrity": "sha256-obzz05lvTvgDh0BHK4ojm0XO92zkrnSA9WicfEOwq9Y="
      },
      {
        "virtualPath": "System.Memory.dll",
        "name": "System.Memory.dll",
        "integrity": "sha256-mAvuvpWjT95vUpoQgI2uDY7P3p0bGQ/s/MdX1MgAA+8="
      },
      {
        "virtualPath": "System.Net.Http.Json.dll",
        "name": "System.Net.Http.Json.dll",
        "integrity": "sha256-bYHg2qGhre+SXK3i3apN6e+E1L40KcTrkF+73HQth54="
      },
      {
        "virtualPath": "System.Net.Http.dll",
        "name": "System.Net.Http.dll",
        "integrity": "sha256-bFYUi58iK///qlu91c6HcjtU29AP6uhcth5YHUdzj2E="
      },
      {
        "virtualPath": "System.Net.HttpListener.dll",
        "name": "System.Net.HttpListener.dll",
        "integrity": "sha256-hd/kJYHxeXJto6pwlRk/8D2i1ND/VhUomkrl5K20FMU="
      },
      {
        "virtualPath": "System.Net.Mail.dll",
        "name": "System.Net.Mail.dll",
        "integrity": "sha256-SIfDyIzcSu1EXqf1QrxBMVsZejUuCazRGaP9KuXQxvo="
      },
      {
        "virtualPath": "System.Net.NameResolution.dll",
        "name": "System.Net.NameResolution.dll",
        "integrity": "sha256-JXjeiJ3wG1xrVdOzBr5BI1KcV9b/mlA8Z30asRN2uQs="
      },
      {
        "virtualPath": "System.Net.NetworkInformation.dll",
        "name": "System.Net.NetworkInformation.dll",
        "integrity": "sha256-Q5P5F3k6P3JbVfyC3VqOzRmVL94XgoC6hNUvWdO7Fkc="
      },
      {
        "virtualPath": "System.Net.Ping.dll",
        "name": "System.Net.Ping.dll",
        "integrity": "sha256-jP7AWMCMY75uKeL5+4eW4Sqy5RcKXFwTksfCa05pzXw="
      },
      {
        "virtualPath": "System.Net.Primitives.dll",
        "name": "System.Net.Primitives.dll",
        "integrity": "sha256-r93mchHuXD0ABbWcj1TsTQcqmMYcj7BryUmF09AdmmU="
      },
      {
        "virtualPath": "System.Net.Quic.dll",
        "name": "System.Net.Quic.dll",
        "integrity": "sha256-TKeCsWav4TsGLAKuCt2HSYiaLkWhT5EgCmcFroghXJ4="
      },
      {
        "virtualPath": "System.Net.Requests.dll",
        "name": "System.Net.Requests.dll",
        "integrity": "sha256-HC2Emdp38jr6ulsMyTrKf2vz/10qgfsXqUWs5T/PAgU="
      },
      {
        "virtualPath": "System.Net.Security.dll",
        "name": "System.Net.Security.dll",
        "integrity": "sha256-fwBcEHddKy/+nVxUN7RBIW8+qwKlJEkbnaUQ3RcJqGE="
      },
      {
        "virtualPath": "System.Net.ServerSentEvents.dll",
        "name": "System.Net.ServerSentEvents.dll",
        "integrity": "sha256-JxXvSGpfzO0XMdzxEUWakZjaGyUG0kXBIC9qRFIHF0c="
      },
      {
        "virtualPath": "System.Net.ServicePoint.dll",
        "name": "System.Net.ServicePoint.dll",
        "integrity": "sha256-UInTnkoqYx1RPZv6jEVT8E/+2xRfn7GsVFDVmOhZ23w="
      },
      {
        "virtualPath": "System.Net.Sockets.dll",
        "name": "System.Net.Sockets.dll",
        "integrity": "sha256-7E9WMU1pXQtEZLVALms3i6IZZqvPqM8sVY45aS9FlOg="
      },
      {
        "virtualPath": "System.Net.WebClient.dll",
        "name": "System.Net.WebClient.dll",
        "integrity": "sha256-uLvCqERRmr2Sk2BrYHwK68vrk2bolzh8zsjzBbRNHmU="
      },
      {
        "virtualPath": "System.Net.WebHeaderCollection.dll",
        "name": "System.Net.WebHeaderCollection.dll",
        "integrity": "sha256-sniVx9fQgYwW66x9Z6WgnYoyQU6bRDZMTe4jHB6IjYQ="
      },
      {
        "virtualPath": "System.Net.WebProxy.dll",
        "name": "System.Net.WebProxy.dll",
        "integrity": "sha256-qgS5lYvwYoXJg0lzFcI8xYOh0ha/sLe3zrI/z6hE8/A="
      },
      {
        "virtualPath": "System.Net.WebSockets.Client.dll",
        "name": "System.Net.WebSockets.Client.dll",
        "integrity": "sha256-N3CJuLKuFH34f4DRankIwh6YmY/XODUXTfQDTIEZhSc="
      },
      {
        "virtualPath": "System.Net.WebSockets.dll",
        "name": "System.Net.WebSockets.dll",
        "integrity": "sha256-t/GVHwSGdf3qpBtoSQAmRGKBGiAuXKsrqCWTp3CSWHE="
      },
      {
        "virtualPath": "System.Net.dll",
        "name": "System.Net.dll",
        "integrity": "sha256-uuR2jAJYkhhYp3G5ScvJKpibrYZiZMSg6zCFOJnzWO8="
      },
      {
        "virtualPath": "System.Numerics.Vectors.dll",
        "name": "System.Numerics.Vectors.dll",
        "integrity": "sha256-yLf0WbHhY/F2NoWuHQ2s3GCAEtXSV9wjX0kVqzidToY="
      },
      {
        "virtualPath": "System.Numerics.dll",
        "name": "System.Numerics.dll",
        "integrity": "sha256-dBRJO7CAvo/tDvB/bfZxSdaHtofqFHcMCAAG/3rSFtc="
      },
      {
        "virtualPath": "System.ObjectModel.dll",
        "name": "System.ObjectModel.dll",
        "integrity": "sha256-Y1GEjTptiPVGIPmSpGIXTwmsHQhEeY3ZMVnGjYe51FU="
      },
      {
        "virtualPath": "System.Private.DataContractSerialization.dll",
        "name": "System.Private.DataContractSerialization.dll",
        "integrity": "sha256-1ZG8OVShW508GV+sitR17VVqOGrLelfUQUmZHqnmPwg="
      },
      {
        "virtualPath": "System.Private.Uri.dll",
        "name": "System.Private.Uri.dll",
        "integrity": "sha256-AAdu22JHFNejvFmf27Qco8ZSDe2PkZWm9qL86/SVU+Y="
      },
      {
        "virtualPath": "System.Private.Xml.Linq.dll",
        "name": "System.Private.Xml.Linq.dll",
        "integrity": "sha256-cIcHzw0tHsDIeRm9aKM9RhjMkfj6oXYOFQpzcCCHgFs="
      },
      {
        "virtualPath": "System.Private.Xml.dll",
        "name": "System.Private.Xml.dll",
        "integrity": "sha256-QaqbBlQRL8rtUX3N0sqI+morXPPLmLjfH6nJjuzhlvs="
      },
      {
        "virtualPath": "System.Reflection.DispatchProxy.dll",
        "name": "System.Reflection.DispatchProxy.dll",
        "integrity": "sha256-r/q4EzgXeRTcXvM/IOpWGl025e21C971Pdr/byRKKR8="
      },
      {
        "virtualPath": "System.Reflection.Emit.ILGeneration.dll",
        "name": "System.Reflection.Emit.ILGeneration.dll",
        "integrity": "sha256-8TVk1nWEw+BCTRZjvfS+kcs45MNazW5pvHWt+Lr+JmA="
      },
      {
        "virtualPath": "System.Reflection.Emit.Lightweight.dll",
        "name": "System.Reflection.Emit.Lightweight.dll",
        "integrity": "sha256-UAtoJdWITu6WSYsUnEZxTGEyc4+X33xdhfkc8rZIBQk="
      },
      {
        "virtualPath": "System.Reflection.Emit.dll",
        "name": "System.Reflection.Emit.dll",
        "integrity": "sha256-4/tBgGbuGGT7efao+73WpYFyLRUoTMQz/Zjyu1e2Vn8="
      },
      {
        "virtualPath": "System.Reflection.Extensions.dll",
        "name": "System.Reflection.Extensions.dll",
        "integrity": "sha256-gxX19DRajBryIg3gOkb3nPNsCDeTyOXdZFwmWbXk6QE="
      },
      {
        "virtualPath": "System.Reflection.Metadata.dll",
        "name": "System.Reflection.Metadata.dll",
        "integrity": "sha256-MBwBvwXaP4K+LU+kn/KaPICYkVYmNAqJf4citJIOFns="
      },
      {
        "virtualPath": "System.Reflection.Primitives.dll",
        "name": "System.Reflection.Primitives.dll",
        "integrity": "sha256-pTlcx1mz+dBdw2rOT3OEETUqsBZR+Z/arQpc5Gx5x7U="
      },
      {
        "virtualPath": "System.Reflection.TypeExtensions.dll",
        "name": "System.Reflection.TypeExtensions.dll",
        "integrity": "sha256-ByokAP9h+YjIk6TgHo30/0K1oEncZ52+z3GL0LnEc9U="
      },
      {
        "virtualPath": "System.Reflection.dll",
        "name": "System.Reflection.dll",
        "integrity": "sha256-0rYxl6Vj7NuLXKce+MJaSUbS7P0/ZUmyon7vnM2DLmM="
      },
      {
        "virtualPath": "System.Resources.Reader.dll",
        "name": "System.Resources.Reader.dll",
        "integrity": "sha256-6CQRe/zSYLJedzlhQ4M444ZfuVOiI8AwXUaiE3cUDNU="
      },
      {
        "virtualPath": "System.Resources.ResourceManager.dll",
        "name": "System.Resources.ResourceManager.dll",
        "integrity": "sha256-+u67Akwg9fTJXGo44pYv+0vg76+OZOXeXgiH5pJgIyQ="
      },
      {
        "virtualPath": "System.Resources.Writer.dll",
        "name": "System.Resources.Writer.dll",
        "integrity": "sha256-/D8czPyn2UlrYJrbEjc7x7lNGVaRMuvbuYSMLQkedZs="
      },
      {
        "virtualPath": "System.Runtime.CompilerServices.Unsafe.dll",
        "name": "System.Runtime.CompilerServices.Unsafe.dll",
        "integrity": "sha256-ysgZ8TtvwwUG+uglfezewbfZGAZQiqfOwyx8Hv2oAaw="
      },
      {
        "virtualPath": "System.Runtime.CompilerServices.VisualC.dll",
        "name": "System.Runtime.CompilerServices.VisualC.dll",
        "integrity": "sha256-pPBBufE8nwdSHgOdn3oMvW68WULVXXpL0TJ84UeHuOg="
      },
      {
        "virtualPath": "System.Runtime.Extensions.dll",
        "name": "System.Runtime.Extensions.dll",
        "integrity": "sha256-v+a5CLK/GANFs8bgIQ6Qxl+dFvWH1hXNR/dnOQj0tUA="
      },
      {
        "virtualPath": "System.Runtime.Handles.dll",
        "name": "System.Runtime.Handles.dll",
        "integrity": "sha256-f+wJAr7hG+GcOkJv7VIlJuivOfsoqH98CH+QXfQ9IEA="
      },
      {
        "virtualPath": "System.Runtime.InteropServices.RuntimeInformation.dll",
        "name": "System.Runtime.InteropServices.RuntimeInformation.dll",
        "integrity": "sha256-6GNCUTP7jo4ynI1tYBIic/akWcsNOK1OvKBYs5bu3Ls="
      },
      {
        "virtualPath": "System.Runtime.InteropServices.dll",
        "name": "System.Runtime.InteropServices.dll",
        "integrity": "sha256-gHeRrnhmm3YZCSBGQqLlQ3y8Li65uWHW71v7s9i0IM4="
      },
      {
        "virtualPath": "System.Runtime.Intrinsics.dll",
        "name": "System.Runtime.Intrinsics.dll",
        "integrity": "sha256-CakSM972pxKnGLpIBr8pDMSaML5JBmk/FtT9XKKc/vc="
      },
      {
        "virtualPath": "System.Runtime.Loader.dll",
        "name": "System.Runtime.Loader.dll",
        "integrity": "sha256-5RGsZ0NLodhW1YtNvbJY8MlOnXF3N+Z4kawzRI14z9o="
      },
      {
        "virtualPath": "System.Runtime.Numerics.dll",
        "name": "System.Runtime.Numerics.dll",
        "integrity": "sha256-LziM/0XrxmCxXT20zFarVmvfuGGzFDki/N2Mfc4koI0="
      },
      {
        "virtualPath": "System.Runtime.Serialization.Formatters.dll",
        "name": "System.Runtime.Serialization.Formatters.dll",
        "integrity": "sha256-mVS33+ku5lmPRa88HxXQpMWbcTJOy+6+FjFWMcnz0WA="
      },
      {
        "virtualPath": "System.Runtime.Serialization.Json.dll",
        "name": "System.Runtime.Serialization.Json.dll",
        "integrity": "sha256-rxL8IOo/ckp+qNt2Cn+8kBmIqvmP7Q7SXhb+lGbtq/c="
      },
      {
        "virtualPath": "System.Runtime.Serialization.Primitives.dll",
        "name": "System.Runtime.Serialization.Primitives.dll",
        "integrity": "sha256-1mDy4rR1+SajKGXd7uBK1Lf0GUW3SQIuQuKSBWIw9GI="
      },
      {
        "virtualPath": "System.Runtime.Serialization.Xml.dll",
        "name": "System.Runtime.Serialization.Xml.dll",
        "integrity": "sha256-TCwaIs7HoM89LGuurJrY7ksaf/C2k70DjhQOSLRX0s4="
      },
      {
        "virtualPath": "System.Runtime.Serialization.dll",
        "name": "System.Runtime.Serialization.dll",
        "integrity": "sha256-fSZHVjJUbVN8IpnRTiz9DI/dqx4YH3r8Es5iWhJrTzs="
      },
      {
        "virtualPath": "System.Runtime.dll",
        "name": "System.Runtime.dll",
        "integrity": "sha256-cwMLAF1H61FrUGCGhIv/FrOU2xpm5MQWb1zqOO3SKGY="
      },
      {
        "virtualPath": "System.Security.AccessControl.dll",
        "name": "System.Security.AccessControl.dll",
        "integrity": "sha256-LmFmP9ZqjWEP2gKJrihJ7Xw5kBOe/TY7lo3Voe9166M="
      },
      {
        "virtualPath": "System.Security.Claims.dll",
        "name": "System.Security.Claims.dll",
        "integrity": "sha256-F7OXTsIDqUh9YK1WVI3gKjnfgnVH5TWIaoZFmOh7wf8="
      },
      {
        "virtualPath": "System.Security.Cryptography.Algorithms.dll",
        "name": "System.Security.Cryptography.Algorithms.dll",
        "integrity": "sha256-fLK+j9Gl4Pk+bwNuTGdDMpxrC5DYfgHhCdzUTDQOYE4="
      },
      {
        "virtualPath": "System.Security.Cryptography.Cng.dll",
        "name": "System.Security.Cryptography.Cng.dll",
        "integrity": "sha256-rtQ7UAQmyEMbeJLE+k+lNDqmesdgccB/pZX8IE6N1Ac="
      },
      {
        "virtualPath": "System.Security.Cryptography.Csp.dll",
        "name": "System.Security.Cryptography.Csp.dll",
        "integrity": "sha256-IdmepTs7U9igTDt2bKqXyCvpoWOvpGtmAeix5AIQbwg="
      },
      {
        "virtualPath": "System.Security.Cryptography.Encoding.dll",
        "name": "System.Security.Cryptography.Encoding.dll",
        "integrity": "sha256-FlzT1RjG10PYo7h67CtI61fW+DUBRcji4N53c2h9UVc="
      },
      {
        "virtualPath": "System.Security.Cryptography.OpenSsl.dll",
        "name": "System.Security.Cryptography.OpenSsl.dll",
        "integrity": "sha256-wu3SbxiyjjRSyqgLvfov4x8xKtBjSZcdtk1pvOAKj+M="
      },
      {
        "virtualPath": "System.Security.Cryptography.Primitives.dll",
        "name": "System.Security.Cryptography.Primitives.dll",
        "integrity": "sha256-Ujg4weMq/tKkCvlaj4T2xTUI0G52Vr5jWjsuD7kE0K0="
      },
      {
        "virtualPath": "System.Security.Cryptography.X509Certificates.dll",
        "name": "System.Security.Cryptography.X509Certificates.dll",
        "integrity": "sha256-2VvSIUUACwhstbQVdP4Z+r15dA9OOEGgOT4kVfL4ahc="
      },
      {
        "virtualPath": "System.Security.Cryptography.dll",
        "name": "System.Security.Cryptography.dll",
        "integrity": "sha256-rrztU+AhzGBw7+vqEjBLToNg9+Ycfx8YVAhnyObtrWY="
      },
      {
        "virtualPath": "System.Security.Principal.Windows.dll",
        "name": "System.Security.Principal.Windows.dll",
        "integrity": "sha256-MEqMsCfM2B0V/jf8Im8gn3149QGyILT2hYseZ+xeEZA="
      },
      {
        "virtualPath": "System.Security.Principal.dll",
        "name": "System.Security.Principal.dll",
        "integrity": "sha256-5Gy1z7SBba8NPsPYa82FsS4Uif39fhZzN1358ZDKUHA="
      },
      {
        "virtualPath": "System.Security.SecureString.dll",
        "name": "System.Security.SecureString.dll",
        "integrity": "sha256-Ny4T/9bjBrxzDAKbb3pTLpdv8oKSGbuw/1Fq1uUxfkI="
      },
      {
        "virtualPath": "System.Security.dll",
        "name": "System.Security.dll",
        "integrity": "sha256-+AjxDibWQJ2cx/H85QiToCHbeIV+92UcZX4cPoezE1Q="
      },
      {
        "virtualPath": "System.ServiceModel.Web.dll",
        "name": "System.ServiceModel.Web.dll",
        "integrity": "sha256-x5i0AKd7a6f6J+kKMFsStkXlbXm6eG1JyEu+iYWDwtk="
      },
      {
        "virtualPath": "System.ServiceProcess.dll",
        "name": "System.ServiceProcess.dll",
        "integrity": "sha256-Uurwymq+TQNio90UphMK7EKOvq8pCpSddWoyDKzvMmI="
      },
      {
        "virtualPath": "System.Text.Encoding.CodePages.dll",
        "name": "System.Text.Encoding.CodePages.dll",
        "integrity": "sha256-LfvYVV60vExjIwxeEnI+i5+6/8Ye9+MuH/Kw7ZmZVcI="
      },
      {
        "virtualPath": "System.Text.Encoding.Extensions.dll",
        "name": "System.Text.Encoding.Extensions.dll",
        "integrity": "sha256-pNICVFKajRpOKnxUiNgS9/TW7Ip2eLt9g8yd1fWnszI="
      },
      {
        "virtualPath": "System.Text.Encoding.dll",
        "name": "System.Text.Encoding.dll",
        "integrity": "sha256-3qAxkoH23Vo9PMta5y3chasw5vQkNgWMHG2NNLU9H/4="
      },
      {
        "virtualPath": "System.Text.Encodings.Web.dll",
        "name": "System.Text.Encodings.Web.dll",
        "integrity": "sha256-gqGsT4t0kjLWz5FZWIJ55sOJ/NBv4PNdWWZDwYKOEQ0="
      },
      {
        "virtualPath": "System.Text.Json.dll",
        "name": "System.Text.Json.dll",
        "integrity": "sha256-A7XPFYq2fjs1B4XFTiA4Y/CuvUKvDuqQmz3vh7qzWNI="
      },
      {
        "virtualPath": "System.Text.RegularExpressions.dll",
        "name": "System.Text.RegularExpressions.dll",
        "integrity": "sha256-sr5vKqDBLH8Ek1QYythNnsUNPhxoTXtE39wjf6p8DNQ="
      },
      {
        "virtualPath": "System.Threading.AccessControl.dll",
        "name": "System.Threading.AccessControl.dll",
        "integrity": "sha256-h4ZtfR2iDuz/bHGt7pVziNwmwC8fj7Dpy5vkBtS4pQ8="
      },
      {
        "virtualPath": "System.Threading.Channels.dll",
        "name": "System.Threading.Channels.dll",
        "integrity": "sha256-XX2NIPqJGa8jGNI+ptKxWeZZYCmBifz0+iCL6asqo9I="
      },
      {
        "virtualPath": "System.Threading.Overlapped.dll",
        "name": "System.Threading.Overlapped.dll",
        "integrity": "sha256-BDuJRYtzBNjJ+YNe2WOyTf2/eoZe06SpYY7DJPKpYks="
      },
      {
        "virtualPath": "System.Threading.Tasks.Dataflow.dll",
        "name": "System.Threading.Tasks.Dataflow.dll",
        "integrity": "sha256-BxO5S9cfCA8waGwMtqKhyQEE0dpzFe47/FR8DU0EleA="
      },
      {
        "virtualPath": "System.Threading.Tasks.Extensions.dll",
        "name": "System.Threading.Tasks.Extensions.dll",
        "integrity": "sha256-8B/suDuV6PxBYWhBdz9PQFoWA9rCa3HJtty6iGo6Fzg="
      },
      {
        "virtualPath": "System.Threading.Tasks.Parallel.dll",
        "name": "System.Threading.Tasks.Parallel.dll",
        "integrity": "sha256-x9iqc/cttN/59/pgWWiCCPupPoUrnIa9DXtNCIXif98="
      },
      {
        "virtualPath": "System.Threading.Tasks.dll",
        "name": "System.Threading.Tasks.dll",
        "integrity": "sha256-AWuJ3znU3DuEYeqBjvLomCva3Nu9+wzmlp8lhcbqA4Q="
      },
      {
        "virtualPath": "System.Threading.Thread.dll",
        "name": "System.Threading.Thread.dll",
        "integrity": "sha256-dASdNLTT6AkZUDKKA+BE4NbTt5n+y1amrkKEYyELUcg="
      },
      {
        "virtualPath": "System.Threading.ThreadPool.dll",
        "name": "System.Threading.ThreadPool.dll",
        "integrity": "sha256-sYKe6F06vQOYqxBHDCSnNAPsbCC037qBvn2UVzaPS90="
      },
      {
        "virtualPath": "System.Threading.Timer.dll",
        "name": "System.Threading.Timer.dll",
        "integrity": "sha256-m61P1X2g1WIP4cr51s/wyXLXKE93JdArD1BRIhZyZIM="
      },
      {
        "virtualPath": "System.Threading.dll",
        "name": "System.Threading.dll",
        "integrity": "sha256-k9Pn3LTYYZfsJOidUduTdDR9EGT3sQqjga1PPMT1FgE="
      },
      {
        "virtualPath": "System.Transactions.Local.dll",
        "name": "System.Transactions.Local.dll",
        "integrity": "sha256-Q+3uXNVVPZqUBrjDnSK9FWJgqGAoeIZDY9jY9lkItyA="
      },
      {
        "virtualPath": "System.Transactions.dll",
        "name": "System.Transactions.dll",
        "integrity": "sha256-Qe4vRpseZGghwR/l7MkNwTdMBj6aZ1qgZQYONI8N5bk="
      },
      {
        "virtualPath": "System.ValueTuple.dll",
        "name": "System.ValueTuple.dll",
        "integrity": "sha256-S3Ursc9mCZIu/Zr898N4kQDSbSHwrrHrB3QrorWiyMw="
      },
      {
        "virtualPath": "System.Web.HttpUtility.dll",
        "name": "System.Web.HttpUtility.dll",
        "integrity": "sha256-TnsuT1LlFbjmMYr6sdOPaNMm/KdIN47XsE2frF/UPtE="
      },
      {
        "virtualPath": "System.Web.dll",
        "name": "System.Web.dll",
        "integrity": "sha256-41SzdnVRmlWRGq827SL15OB8Ty2qUlH2hdYmwosxw4c="
      },
      {
        "virtualPath": "System.Windows.dll",
        "name": "System.Windows.dll",
        "integrity": "sha256-ET/7T9wVxb5AsHilK97k5rchbY4mWa235jc1kARnQNY="
      },
      {
        "virtualPath": "System.Xml.Linq.dll",
        "name": "System.Xml.Linq.dll",
        "integrity": "sha256-Lp6npMmdI+VQFcsUcNWQrA2J+OPe7Vov5glySY8QJXU="
      },
      {
        "virtualPath": "System.Xml.ReaderWriter.dll",
        "name": "System.Xml.ReaderWriter.dll",
        "integrity": "sha256-pYZ/ruW9X1r2TWE/x30UB2GxxyClqD0+Vztv0lusbSc="
      },
      {
        "virtualPath": "System.Xml.Serialization.dll",
        "name": "System.Xml.Serialization.dll",
        "integrity": "sha256-kPiy7hw6e+jk551antAMhmMN1Q4il6TAUp4yBNXn8Kk="
      },
      {
        "virtualPath": "System.Xml.XDocument.dll",
        "name": "System.Xml.XDocument.dll",
        "integrity": "sha256-3vASFt3byIGp7mhsSBBqM35HTgl7sEbRzroveswilek="
      },
      {
        "virtualPath": "System.Xml.XPath.XDocument.dll",
        "name": "System.Xml.XPath.XDocument.dll",
        "integrity": "sha256-7ZqgKHHHd0sJXB7I8cgrYrReFzGglD1ejh/cWPnuriE="
      },
      {
        "virtualPath": "System.Xml.XPath.dll",
        "name": "System.Xml.XPath.dll",
        "integrity": "sha256-nPF+n7/cBFtxOjAgwLP8S6en+TtdUuPUgkx+0ESKV8U="
      },
      {
        "virtualPath": "System.Xml.XmlDocument.dll",
        "name": "System.Xml.XmlDocument.dll",
        "integrity": "sha256-A6lGjbSAotI8BCB1fP1N4EUlxhEF9wbmTnz7ffCbXOk="
      },
      {
        "virtualPath": "System.Xml.XmlSerializer.dll",
        "name": "System.Xml.XmlSerializer.dll",
        "integrity": "sha256-4rV2x4Gmo7jXIY76FRfY+qo0NDySXEhJqIV7q/H4Rg0="
      },
      {
        "virtualPath": "System.Xml.dll",
        "name": "System.Xml.dll",
        "integrity": "sha256-zntokeQGXC4uIhYQJVNDq1pIw95EE81R/auuZYBiaD8="
      },
      {
        "virtualPath": "System.dll",
        "name": "System.dll",
        "integrity": "sha256-+OZyfynDrWACSLvcWVWL+jPg16TXGgP9N8gub/xKoFc="
      },
      {
        "virtualPath": "WindowsBase.dll",
        "name": "WindowsBase.dll",
        "integrity": "sha256-btr21ARMl9a4hsb+/RALJnVAZQ6HAFRKDC0CogEhbrE="
      },
      {
        "virtualPath": "Yaml2JsonNode.dll",
        "name": "Yaml2JsonNode.dll",
        "integrity": "sha256-iTSsYlPuBKBEc5agUMP6TvtFjfPL++RZdH3d6YSflLg="
      },
      {
        "virtualPath": "YamlDotNet.dll",
        "name": "YamlDotNet.dll",
        "integrity": "sha256-hvhz9oLGGaDCTiY9zdFf4CcQRmZkHJL1A1FmcdYe3aY="
      },
      {
        "virtualPath": "json-everything.net.dll",
        "name": "json-everything.net.dll",
        "integrity": "sha256-3szpmRFE/tbMmnCyg40mHORnWYThHE4qrIyRItOlLos="
      },
      {
        "virtualPath": "mscorlib.dll",
        "name": "mscorlib.dll",
        "integrity": "sha256-PHyV0yKtWPmRM8XkhD7ywWR+39eynVFSi16jgWgaoUk="
      },
      {
        "virtualPath": "netstandard.dll",
        "name": "netstandard.dll",
        "integrity": "sha256-Jg18hIMEwner0ACW4TRsKsgCRTryNMaaDnTLdskmaus="
      }
    ],
    "satelliteResources": {
      "cs": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-k2NSuYEM/fQ9m1vFwJmvMuOZVDu7GyOgiwj+3SkevwE="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-ikmBWI2ZGOZ7ZSQhnD1OlMepjJQ7h3rdYSoZXDDEX2o="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-wxUW99lCrxHVLS8B4TykziDIycCeLYtDOSB1euwmKl0="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-qgKSA7u9UBkC/nvXK0BziRZ23l/Fme4+bul8XrpCN/0="
        }
      ],
      "de": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-Yk+N34/4kzRktdYy6QZ5u+NjJJ5uDiKck56sSCL5NE8="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-q1qEGYgP6NdW3uGBaXTLE1jc7tCTpHGfoUXtVlDHO2Y="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-DaW9dUo0tQcDMS+piNEkKfeMhwKRmxhndXFzmT8zPbw="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-fl/a24LrwLuwXUtGmx8Mp4wc7Z/Wbl1RY7ud7uGruts="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-lwvwO8QZWkiqN4Aj/duCV4lYcQPyfCanLnpgBQlkmqU="
        }
      ],
      "es": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-u2fddAg4/r6MQR7FoGhtcBz6ihX+mXHUX9ve730lBdA="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-ckR4Ujz/fg0wk8bL9Mw2Cx3CNdkfLCgf3VDyB7WvxRg="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-CQ4Pq7FL/qs4b3ESmQa09oeh12SFd23AmzfO7+UQyhY="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-T9c48j2p9TwqwdfapxbqvnLycR9eV+C1VqRu6ZbV+yw="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-E4SCFhf8gxjahzITYUqAtU8wa7PGRIlkdeHqbixeoiM="
        }
      ],
      "fr": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-S/8tFwtVGGvWUFmPUcgvwamKW6z0tpzV5baw/zt39bY="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-o7vj9E3tSYErQo73ya7/HmW0IwtElt9ptK3WV9OEKMI="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-Byhshe6RntAIFJnCx/xMg+t0fs3uJZ/2Q7sH7KnFXeU="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-a4vWUNwFry/cBrf/Ci6gX0kT2cgKf109uQWvksIC8kU="
        }
      ],
      "it": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-N2/ssoo3Tby+6Fi2frty2Yo0OVFAnS8kaXStAHdFMZI="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-4HbdhvsLvjxHhXl0c6UdPzP/uFvdreEcMpXrWCA7UDY="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-B5h0YTD417oLox+8sztK9nwt0FApB5Olq58dvwWUB9s="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-tezIHPBxr8C4Fv+nNRDAywIrZ/ANOXNB3CQ4lJNgJqk="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-t4SbPTQBTKxr7fYL63yRZfNE5Ai4zTOmUcY3X+WFbds="
        }
      ],
      "ja": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-IjzFM6q6qWQbs+kOJJGFAkdaY59wOVG+xlZd5XCZaMs="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-jr3vwf5CXOlnOvxSPPiSJYkk35Z4CdImDRjiOJwwR7o="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-xbA7v8OiN+aMgavq8BdyA7IhYNCCPeh/gzJDtddyRXs="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-80VX2R3RdsIEbdqAx9Zmzt002Ubrq8ekHtbh8trXaZU="
        }
      ],
      "ko": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-/tVz+71tFpr0WxTTBMfbiJeKeh7VbKVxxbIek25WpkY="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-wTyOIH9IrAS1q3YZLWHwSUkNfT5QslUBEaFFVhmbLGo="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-WEuF+1YUBF2kigGSrg44CawBtYxlFwFi5EEKucRsuNc="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-dsP6vwssWm7vooM2HoNhULCYmqoFNLSsmjblpeboma0="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-eSUEcH+3HwKv8tEuStIf40tZnri1b9LZ5aFLPUsDEdc="
        }
      ],
      "nb-NO": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-707ExFIrDWKi+2m+R/xlNdtY66oDLjgBYFEEuRFbrqQ="
        }
      ],
      "pl-PL": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-gT7kpy2fGykXnrgHJ1cPLfkoEI8+nNhl0yQhqAz2GwI="
        }
      ],
      "pl": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-A/RcacJkvdoS56n8q+ahfltqAOkPdgJo3N5Dt0QsCns="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-troXBW9Go3HosgjrbwwR/h7rJHuadPDWHm1lUn2eOjg="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-JZKDkBrrWAP/kIgeloyjLUBwx45XCg7TrKNvSUE7LtE="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-mjuEcdrQLApK/cKBs24cV0O884yHB/tKWjKLkzLlFaI="
        }
      ],
      "pt-BR": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-p3YqcSytzEiaF73EMeb84QnDfc0vjmZ0LIWRUzO+rLs="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-JI+s7ZoudZ8eQ/m9J1jdOv5ygnkZRjO0F9AthlBUBps="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-iFCWf6PHeEbEugNFb0Qcl7Y7g3Cpo97cKZjELjUeD1c="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-ZNUehGQ8trHf+oZAzr1xhJOlhUXO8pUIKqS83UPE3q0="
        }
      ],
      "pt": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-22ii8Lt3stl1ndRTd6h2iupSe+gQ2JsHw1r1wec/Ujo="
        }
      ],
      "ru": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-StD3Y/CVBLpp7uhHV7pO99ZFDFNtwP1eFGpmquMFjQo="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-qfbIox3FtEFatyvc+FtZZstBf7rf0gFuztAiRKW2eGo="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-7hb/te8KLWPthszI7f0xo99XmOf4ZzKY7XGp/XoHM50="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-u3C0l4+71FgkxwpSLIaN8/lBfzvUbzqmmTnuMPuAiEU="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-lwJF8o7o1lO6vg249OE99F3W42p1QhnPhjy2fulbml0="
        }
      ],
      "sv-SE": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-Qd28CFr5ObQByMwDBuFqlbfJ7hTGiqZHA+ODpXZxZJw="
        }
      ],
      "tr-TR": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-h6FMr615vgrzAgMWqKqSY3mVNrel4h3ARFn8+7e6WH8="
        }
      ],
      "tr": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-SUcsj77Mey7FcgbUmZF4423GE5IRpnR3tefnlRS2TY0="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-UVkXOAD5fXZ/le/wcPoaRLIDifgWgIJTM9eS2l4dYvo="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-a+1Jh48U7bbHz/mzXr8Y3UVWUA+DKRFaMfyG/ta4kgk="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-xx41DYR5HBXKpIl0k9YylvzxwGHxGA3PLSz6Yujo8VI="
        }
      ],
      "zh-Hans": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-tYltt73O8i9Wdbq+A0hSnROHJdnyKK/Ajc0lKqmSRBE="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-W7F/Kfgn7A4BUm5grq2kZSKdssaVe6J5mhcRQr2XCzM="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-0Em03P4cIqqd6ktTjjHOn8MnfiOw/Of+eaP+yKrVgb0="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-N9nmkleh1Y6LGw89tAxFzR5I0Qm5v81sSWyBidHe444="
        }
      ],
      "zh-Hant": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-F48HRo3A46zc5rYqRq577dL3e/34ouM1ZmKBQlIU9f0="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-HGKL6sa9XeZHeDrlDfZnnI3RbIuql38IsDUTUeMfmhQ="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-Sjnve5mS0BVRu5YWUvC8ZjM1vaxRPUwDj1b1mZWs+pM="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-M5ho19+HL3rPk2R3XX+s5Zs2Gs5TVIBIF0XnNXyXSBs="
        }
      ]
    },
    "libraryInitializers": [
      {
        "name": "_content/Microsoft.DotNet.HotReload.WebAssembly.Browser/Microsoft.DotNet.HotReload.WebAssembly.Browser.99zm1jdh75.lib.module.js"
      }
    ],
    "modulesAfterConfigLoaded": [
      {
        "name": "../_content/Microsoft.DotNet.HotReload.WebAssembly.Browser/Microsoft.DotNet.HotReload.WebAssembly.Browser.99zm1jdh75.lib.module.js"
      }
    ]
  },
  "debugLevel": 0,
  "globalizationMode": "sharded",
  "extensions": {
    "blazor": {}
  },
  "runtimeConfig": {
    "runtimeOptions": {
      "configProperties": {
        "Microsoft.AspNetCore.Components.Routing.RegexConstraintSupport": false,
        "System.Diagnostics.Debugger.IsSupported": false,
        "System.Diagnostics.Metrics.Meter.IsSupported": false,
        "System.Diagnostics.Tracing.EventSource.IsSupported": false,
        "System.GC.Server": true,
        "System.Globalization.Invariant": false,
        "System.TimeZoneInfo.Invariant": false,
        "System.Linq.Enumerable.IsSizeOptimized": true,
        "System.Net.Http.EnableActivityPropagation": false,
        "System.Net.Http.WasmEnableStreamingResponse": true,
        "System.Net.SocketsHttpHandler.Http3Support": false,
        "System.Reflection.Metadata.MetadataUpdater.IsSupported": false,
        "System.Resources.UseSystemResourceKeys": true,
        "System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization": false,
        "System.Text.Encoding.EnableUnsafeUTF7Encoding": false,
        "System.Text.Json.JsonSerializer.IsReflectionEnabledByDefault": true
      }
    }
  }
}/*json-end*/);export{gt as default,ft as dotnet,mt as exit};
