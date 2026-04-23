//! Licensed to the .NET Foundation under one or more agreements.
//! The .NET Foundation licenses this file to you under the MIT license.

var e=!1;const t=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,4,1,96,0,0,3,2,1,0,10,8,1,6,0,6,64,25,11,11])),o=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,5,1,96,0,1,123,3,2,1,0,10,15,1,13,0,65,1,253,15,65,2,253,15,253,128,2,11])),n=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,5,1,96,0,1,123,3,2,1,0,10,10,1,8,0,65,0,253,15,253,98,11])),r=Symbol.for("wasm promise_control");function i(e,t){let o=null;const n=new Promise((function(n,r){o={isDone:!1,promise:null,resolve:t=>{o.isDone||(o.isDone=!0,n(t),e&&e())},reject:e=>{o.isDone||(o.isDone=!0,r(e),t&&t())}}}));o.promise=n;const i=n;return i[r]=o,{promise:i,promise_control:o}}function s(e){return e[r]}function a(e){e&&function(e){return void 0!==e[r]}(e)||Be(!1,"Promise is not controllable")}const l="__mono_message__",c=["debug","log","trace","warn","info","error"],d="MONO_WASM: ";let u,f,m,g,p,h;function w(e){g=e}function b(e){if(Pe.diagnosticTracing){const t="function"==typeof e?e():e;console.debug(d+t)}}function y(e,...t){console.info(d+e,...t)}function v(e,...t){console.info(e,...t)}function E(e,...t){console.warn(d+e,...t)}function _(e,...t){if(t&&t.length>0&&t[0]&&"object"==typeof t[0]){if(t[0].silent)return;if(t[0].toString)return void console.error(d+e,t[0].toString())}console.error(d+e,...t)}function x(e,t,o){return function(...n){try{let r=n[0];if(void 0===r)r="undefined";else if(null===r)r="null";else if("function"==typeof r)r=r.toString();else if("string"!=typeof r)try{r=JSON.stringify(r)}catch(e){r=r.toString()}t(o?JSON.stringify({method:e,payload:r,arguments:n.slice(1)}):[e+r,...n.slice(1)])}catch(e){m.error(`proxyConsole failed: ${e}`)}}}function j(e,t,o){f=t,g=e,m={...t};const n=`${o}/console`.replace("https://","wss://").replace("http://","ws://");u=new WebSocket(n),u.addEventListener("error",A),u.addEventListener("close",S),function(){for(const e of c)f[e]=x(`console.${e}`,T,!0)}()}function R(e){let t=30;const o=()=>{u?0==u.bufferedAmount||0==t?(e&&v(e),function(){for(const e of c)f[e]=x(`console.${e}`,m.log,!1)}(),u.removeEventListener("error",A),u.removeEventListener("close",S),u.close(1e3,e),u=void 0):(t--,globalThis.setTimeout(o,100)):e&&m&&m.log(e)};o()}function T(e){u&&u.readyState===WebSocket.OPEN?u.send(e):m.log(e)}function A(e){m.error(`[${g}] proxy console websocket error: ${e}`,e)}function S(e){m.debug(`[${g}] proxy console websocket closed: ${e}`,e)}function D(){Pe.preferredIcuAsset=O(Pe.config);let e="invariant"==Pe.config.globalizationMode;if(!e)if(Pe.preferredIcuAsset)Pe.diagnosticTracing&&b("ICU data archive(s) available, disabling invariant mode");else{if("custom"===Pe.config.globalizationMode||"all"===Pe.config.globalizationMode||"sharded"===Pe.config.globalizationMode){const e="invariant globalization mode is inactive and no ICU data archives are available";throw _(`ERROR: ${e}`),new Error(e)}Pe.diagnosticTracing&&b("ICU data archive(s) not available, using invariant globalization mode"),e=!0,Pe.preferredIcuAsset=null}const t="DOTNET_SYSTEM_GLOBALIZATION_INVARIANT",o=Pe.config.environmentVariables;if(void 0===o[t]&&e&&(o[t]="1"),void 0===o.TZ)try{const e=Intl.DateTimeFormat().resolvedOptions().timeZone||null;e&&(o.TZ=e)}catch(e){y("failed to detect timezone, will fallback to UTC")}}function O(e){var t;if((null===(t=e.resources)||void 0===t?void 0:t.icu)&&"invariant"!=e.globalizationMode){const t=e.applicationCulture||(ke?globalThis.navigator&&globalThis.navigator.languages&&globalThis.navigator.languages[0]:Intl.DateTimeFormat().resolvedOptions().locale),o=e.resources.icu;let n=null;if("custom"===e.globalizationMode){if(o.length>=1)return o[0].name}else t&&"all"!==e.globalizationMode?"sharded"===e.globalizationMode&&(n=function(e){const t=e.split("-")[0];return"en"===t||["fr","fr-FR","it","it-IT","de","de-DE","es","es-ES"].includes(e)?"icudt_EFIGS.dat":["zh","ko","ja"].includes(t)?"icudt_CJK.dat":"icudt_no_CJK.dat"}(t)):n="icudt.dat";if(n)for(let e=0;e<o.length;e++){const t=o[e];if(t.virtualPath===n)return t.name}}return e.globalizationMode="invariant",null}(new Date).valueOf();const C=class{constructor(e){this.url=e}toString(){return this.url}};async function k(e,t){try{const o="function"==typeof globalThis.fetch;if(Se){const n=e.startsWith("file://");if(!n&&o)return globalThis.fetch(e,t||{credentials:"same-origin"});p||(h=Ne.require("url"),p=Ne.require("fs")),n&&(e=h.fileURLToPath(e));const r=await p.promises.readFile(e);return{ok:!0,headers:{length:0,get:()=>null},url:e,arrayBuffer:()=>r,json:()=>JSON.parse(r),text:()=>{throw new Error("NotImplementedException")}}}if(o)return globalThis.fetch(e,t||{credentials:"same-origin"});if("function"==typeof read)return{ok:!0,url:e,headers:{length:0,get:()=>null},arrayBuffer:()=>new Uint8Array(read(e,"binary")),json:()=>JSON.parse(read(e,"utf8")),text:()=>read(e,"utf8")}}catch(t){return{ok:!1,url:e,status:500,headers:{length:0,get:()=>null},statusText:"ERR28: "+t,arrayBuffer:()=>{throw t},json:()=>{throw t},text:()=>{throw t}}}throw new Error("No fetch implementation available")}function I(e){return"string"!=typeof e&&Be(!1,"url must be a string"),!M(e)&&0!==e.indexOf("./")&&0!==e.indexOf("../")&&globalThis.URL&&globalThis.document&&globalThis.document.baseURI&&(e=new URL(e,globalThis.document.baseURI).toString()),e}const U=/^[a-zA-Z][a-zA-Z\d+\-.]*?:\/\//,P=/[a-zA-Z]:[\\/]/;function M(e){return Se||Ie?e.startsWith("/")||e.startsWith("\\")||-1!==e.indexOf("///")||P.test(e):U.test(e)}let L,N=0;const $=[],z=[],W=new Map,F={"js-module-threads":!0,"js-module-runtime":!0,"js-module-dotnet":!0,"js-module-native":!0,"js-module-diagnostics":!0},B={...F,"js-module-library-initializer":!0},V={...F,dotnetwasm:!0,heap:!0,manifest:!0},q={...B,manifest:!0},H={...B,dotnetwasm:!0},J={dotnetwasm:!0,symbols:!0},Z={...B,dotnetwasm:!0,symbols:!0},Q={symbols:!0};function G(e){return!("icu"==e.behavior&&e.name!=Pe.preferredIcuAsset)}function K(e,t,o){null!=t||(t=[]),Be(1==t.length,`Expect to have one ${o} asset in resources`);const n=t[0];return n.behavior=o,X(n),e.push(n),n}function X(e){V[e.behavior]&&W.set(e.behavior,e)}function Y(e){Be(V[e],`Unknown single asset behavior ${e}`);const t=W.get(e);if(t&&!t.resolvedUrl)if(t.resolvedUrl=Pe.locateFile(t.name),F[t.behavior]){const e=ge(t);e?("string"!=typeof e&&Be(!1,"loadBootResource response for 'dotnetjs' type should be a URL string"),t.resolvedUrl=e):t.resolvedUrl=ce(t.resolvedUrl,t.behavior)}else if("dotnetwasm"!==t.behavior)throw new Error(`Unknown single asset behavior ${e}`);return t}function ee(e){const t=Y(e);return Be(t,`Single asset for ${e} not found`),t}let te=!1;async function oe(){if(!te){te=!0,Pe.diagnosticTracing&&b("mono_download_assets");try{const e=[],t=[],o=(e,t)=>{!Z[e.behavior]&&G(e)&&Pe.expected_instantiated_assets_count++,!H[e.behavior]&&G(e)&&(Pe.expected_downloaded_assets_count++,t.push(se(e)))};for(const t of $)o(t,e);for(const e of z)o(e,t);Pe.allDownloadsQueued.promise_control.resolve(),Promise.all([...e,...t]).then((()=>{Pe.allDownloadsFinished.promise_control.resolve()})).catch((e=>{throw Pe.err("Error in mono_download_assets: "+e),Xe(1,e),e})),await Pe.runtimeModuleLoaded.promise;const n=async e=>{const t=await e;if(t.buffer){if(!Z[t.behavior]){t.buffer&&"object"==typeof t.buffer||Be(!1,"asset buffer must be array-like or buffer-like or promise of these"),"string"!=typeof t.resolvedUrl&&Be(!1,"resolvedUrl must be string");const e=t.resolvedUrl,o=await t.buffer,n=new Uint8Array(o);pe(t),await Ue.beforeOnRuntimeInitialized.promise,Ue.instantiate_asset(t,e,n)}}else J[t.behavior]?("symbols"===t.behavior&&(await Ue.instantiate_symbols_asset(t),pe(t)),J[t.behavior]&&++Pe.actual_downloaded_assets_count):(t.isOptional||Be(!1,"Expected asset to have the downloaded buffer"),!H[t.behavior]&&G(t)&&Pe.expected_downloaded_assets_count--,!Z[t.behavior]&&G(t)&&Pe.expected_instantiated_assets_count--)},r=[],i=[];for(const t of e)r.push(n(t));for(const e of t)i.push(n(e));Promise.all(r).then((()=>{Ce||Ue.coreAssetsInMemory.promise_control.resolve()})).catch((e=>{throw Pe.err("Error in mono_download_assets: "+e),Xe(1,e),e})),Promise.all(i).then((async()=>{Ce||(await Ue.coreAssetsInMemory.promise,Ue.allAssetsInMemory.promise_control.resolve())})).catch((e=>{throw Pe.err("Error in mono_download_assets: "+e),Xe(1,e),e}))}catch(e){throw Pe.err("Error in mono_download_assets: "+e),e}}}let ne=!1;function re(){if(ne)return;ne=!0;const e=Pe.config,t=[];if(e.assets)for(const t of e.assets)"object"!=typeof t&&Be(!1,`asset must be object, it was ${typeof t} : ${t}`),"string"!=typeof t.behavior&&Be(!1,"asset behavior must be known string"),"string"!=typeof t.name&&Be(!1,"asset name must be string"),t.resolvedUrl&&"string"!=typeof t.resolvedUrl&&Be(!1,"asset resolvedUrl could be string"),t.hash&&"string"!=typeof t.hash&&Be(!1,"asset resolvedUrl could be string"),t.pendingDownload&&"object"!=typeof t.pendingDownload&&Be(!1,"asset pendingDownload could be object"),t.isCore?$.push(t):z.push(t),X(t);else if(e.resources){const o=e.resources;o.wasmNative||Be(!1,"resources.wasmNative must be defined"),o.jsModuleNative||Be(!1,"resources.jsModuleNative must be defined"),o.jsModuleRuntime||Be(!1,"resources.jsModuleRuntime must be defined"),K(z,o.wasmNative,"dotnetwasm"),K(t,o.jsModuleNative,"js-module-native"),K(t,o.jsModuleRuntime,"js-module-runtime"),o.jsModuleDiagnostics&&K(t,o.jsModuleDiagnostics,"js-module-diagnostics");const n=(e,t,o)=>{const n=e;n.behavior=t,o?(n.isCore=!0,$.push(n)):z.push(n)};if(o.coreAssembly)for(let e=0;e<o.coreAssembly.length;e++)n(o.coreAssembly[e],"assembly",!0);if(o.assembly)for(let e=0;e<o.assembly.length;e++)n(o.assembly[e],"assembly",!o.coreAssembly);if(0!=e.debugLevel&&Pe.isDebuggingSupported()){if(o.corePdb)for(let e=0;e<o.corePdb.length;e++)n(o.corePdb[e],"pdb",!0);if(o.pdb)for(let e=0;e<o.pdb.length;e++)n(o.pdb[e],"pdb",!o.corePdb)}if(e.loadAllSatelliteResources&&o.satelliteResources)for(const e in o.satelliteResources)for(let t=0;t<o.satelliteResources[e].length;t++){const r=o.satelliteResources[e][t];r.culture=e,n(r,"resource",!o.coreAssembly)}if(o.coreVfs)for(let e=0;e<o.coreVfs.length;e++)n(o.coreVfs[e],"vfs",!0);if(o.vfs)for(let e=0;e<o.vfs.length;e++)n(o.vfs[e],"vfs",!o.coreVfs);const r=O(e);if(r&&o.icu)for(let e=0;e<o.icu.length;e++){const t=o.icu[e];t.name===r&&n(t,"icu",!1)}if(o.wasmSymbols)for(let e=0;e<o.wasmSymbols.length;e++)n(o.wasmSymbols[e],"symbols",!1)}if(e.appsettings)for(let t=0;t<e.appsettings.length;t++){const o=e.appsettings[t],n=he(o);"appsettings.json"!==n&&n!==`appsettings.${e.applicationEnvironment}.json`||z.push({name:o,behavior:"vfs",cache:"no-cache",useCredentials:!0})}e.assets=[...$,...z,...t]}async function ie(e){const t=await se(e);return await t.pendingDownloadInternal.response,t.buffer}async function se(e){try{return await ae(e)}catch(t){if(!Pe.enableDownloadRetry)throw t;if(Ie||Se)throw t;if(e.pendingDownload&&e.pendingDownloadInternal==e.pendingDownload)throw t;if(e.resolvedUrl&&-1!=e.resolvedUrl.indexOf("file://"))throw t;if(t&&404==t.status)throw t;e.pendingDownloadInternal=void 0,await Pe.allDownloadsQueued.promise;try{return Pe.diagnosticTracing&&b(`Retrying download '${e.name}'`),await ae(e)}catch(t){return e.pendingDownloadInternal=void 0,await new Promise((e=>globalThis.setTimeout(e,100))),Pe.diagnosticTracing&&b(`Retrying download (2) '${e.name}' after delay`),await ae(e)}}}async function ae(e){for(;L;)await L.promise;try{++N,N==Pe.maxParallelDownloads&&(Pe.diagnosticTracing&&b("Throttling further parallel downloads"),L=i());const t=await async function(e){if(e.pendingDownload&&(e.pendingDownloadInternal=e.pendingDownload),e.pendingDownloadInternal&&e.pendingDownloadInternal.response)return e.pendingDownloadInternal.response;if(e.buffer){const t=await e.buffer;return e.resolvedUrl||(e.resolvedUrl="undefined://"+e.name),e.pendingDownloadInternal={url:e.resolvedUrl,name:e.name,response:Promise.resolve({ok:!0,arrayBuffer:()=>t,json:()=>JSON.parse(new TextDecoder("utf-8").decode(t)),text:()=>{throw new Error("NotImplementedException")},headers:{get:()=>{}}})},e.pendingDownloadInternal.response}const t=e.loadRemote&&Pe.config.remoteSources?Pe.config.remoteSources:[""];let o;for(let n of t){n=n.trim(),"./"===n&&(n="");const t=le(e,n);e.name===t?Pe.diagnosticTracing&&b(`Attempting to download '${t}'`):Pe.diagnosticTracing&&b(`Attempting to download '${t}' for ${e.name}`);try{e.resolvedUrl=t;const n=fe(e);if(e.pendingDownloadInternal=n,o=await n.response,!o||!o.ok)continue;return o}catch(e){o||(o={ok:!1,url:t,status:0,statusText:""+e});continue}}const n=e.isOptional||e.name.match(/\.pdb$/)&&Pe.config.ignorePdbLoadErrors;if(o||Be(!1,`Response undefined ${e.name}`),!n){const t=new Error(`download '${o.url}' for ${e.name} failed ${o.status} ${o.statusText}`);throw t.status=o.status,t}y(`optional download '${o.url}' for ${e.name} failed ${o.status} ${o.statusText}`)}(e);return t?(J[e.behavior]||(e.buffer=await t.arrayBuffer(),++Pe.actual_downloaded_assets_count),e):e}finally{if(--N,L&&N==Pe.maxParallelDownloads-1){Pe.diagnosticTracing&&b("Resuming more parallel downloads");const e=L;L=void 0,e.promise_control.resolve()}}}function le(e,t){let o;return null==t&&Be(!1,`sourcePrefix must be provided for ${e.name}`),e.resolvedUrl?o=e.resolvedUrl:(o=""===t?"assembly"===e.behavior||"pdb"===e.behavior?e.name:"resource"===e.behavior&&e.culture&&""!==e.culture?`${e.culture}/${e.name}`:e.name:t+e.name,o=ce(Pe.locateFile(o),e.behavior)),o&&"string"==typeof o||Be(!1,"attemptUrl need to be path or url string"),o}function ce(e,t){return Pe.modulesUniqueQuery&&q[t]&&(e+=Pe.modulesUniqueQuery),e}let de=0;const ue=new Set;function fe(e){try{e.resolvedUrl||Be(!1,"Request's resolvedUrl must be set");const t=function(e){let t=e.resolvedUrl;if(Pe.loadBootResource){const o=ge(e);if(o instanceof Promise)return o;"string"==typeof o&&(t=o)}const o={};return e.cache?o.cache=e.cache:Pe.config.disableNoCacheFetch||(o.cache="no-cache"),e.useCredentials?o.credentials="include":!Pe.config.disableIntegrityCheck&&e.hash&&(o.integrity=e.hash),Pe.fetch_like(t,o)}(e),o={name:e.name,url:e.resolvedUrl,response:t};return ue.add(e.name),o.response.then((()=>{"assembly"==e.behavior&&Pe.loadedAssemblies.push(e.name),de++,Pe.onDownloadResourceProgress&&Pe.onDownloadResourceProgress(de,ue.size)})),o}catch(t){const o={ok:!1,url:e.resolvedUrl,status:500,statusText:"ERR29: "+t,arrayBuffer:()=>{throw t},json:()=>{throw t}};return{name:e.name,url:e.resolvedUrl,response:Promise.resolve(o)}}}const me={resource:"assembly",assembly:"assembly",pdb:"pdb",icu:"globalization",vfs:"configuration",manifest:"manifest",dotnetwasm:"dotnetwasm","js-module-dotnet":"dotnetjs","js-module-native":"dotnetjs","js-module-runtime":"dotnetjs","js-module-threads":"dotnetjs"};function ge(e){var t;if(Pe.loadBootResource){const o=null!==(t=e.hash)&&void 0!==t?t:"",n=e.resolvedUrl,r=me[e.behavior];if(r){const t=Pe.loadBootResource(r,e.name,n,o,e.behavior);return"string"==typeof t?I(t):t}}}function pe(e){e.pendingDownloadInternal=null,e.pendingDownload=null,e.buffer=null,e.moduleExports=null}function he(e){let t=e.lastIndexOf("/");return t>=0&&t++,e.substring(t)}async function we(e){e&&await Promise.all((null!=e?e:[]).map((e=>async function(e){try{const t=e.name;if(!e.moduleExports){const o=ce(Pe.locateFile(t),"js-module-library-initializer");Pe.diagnosticTracing&&b(`Attempting to import '${o}' for ${e}`),e.moduleExports=await import(/*! webpackIgnore: true */o)}Pe.libraryInitializers.push({scriptName:t,exports:e.moduleExports})}catch(t){E(`Failed to import library initializer '${e}': ${t}`)}}(e))))}async function be(e,t){if(!Pe.libraryInitializers)return;const o=[];for(let n=0;n<Pe.libraryInitializers.length;n++){const r=Pe.libraryInitializers[n];r.exports[e]&&o.push(ye(r.scriptName,e,(()=>r.exports[e](...t))))}await Promise.all(o)}async function ye(e,t,o){try{await o()}catch(o){throw E(`Failed to invoke '${t}' on library initializer '${e}': ${o}`),Xe(1,o),o}}function ve(e,t){if(e===t)return e;const o={...t};return void 0!==o.assets&&o.assets!==e.assets&&(o.assets=[...e.assets||[],...o.assets||[]]),void 0!==o.resources&&(o.resources=_e(e.resources||{assembly:[],jsModuleNative:[],jsModuleRuntime:[],wasmNative:[]},o.resources)),void 0!==o.environmentVariables&&(o.environmentVariables={...e.environmentVariables||{},...o.environmentVariables||{}}),void 0!==o.runtimeOptions&&o.runtimeOptions!==e.runtimeOptions&&(o.runtimeOptions=[...e.runtimeOptions||[],...o.runtimeOptions||[]]),Object.assign(e,o)}function Ee(e,t){if(e===t)return e;const o={...t};return o.config&&(e.config||(e.config={}),o.config=ve(e.config,o.config)),Object.assign(e,o)}function _e(e,t){if(e===t)return e;const o={...t};return void 0!==o.coreAssembly&&(o.coreAssembly=[...e.coreAssembly||[],...o.coreAssembly||[]]),void 0!==o.assembly&&(o.assembly=[...e.assembly||[],...o.assembly||[]]),void 0!==o.lazyAssembly&&(o.lazyAssembly=[...e.lazyAssembly||[],...o.lazyAssembly||[]]),void 0!==o.corePdb&&(o.corePdb=[...e.corePdb||[],...o.corePdb||[]]),void 0!==o.pdb&&(o.pdb=[...e.pdb||[],...o.pdb||[]]),void 0!==o.jsModuleWorker&&(o.jsModuleWorker=[...e.jsModuleWorker||[],...o.jsModuleWorker||[]]),void 0!==o.jsModuleNative&&(o.jsModuleNative=[...e.jsModuleNative||[],...o.jsModuleNative||[]]),void 0!==o.jsModuleDiagnostics&&(o.jsModuleDiagnostics=[...e.jsModuleDiagnostics||[],...o.jsModuleDiagnostics||[]]),void 0!==o.jsModuleRuntime&&(o.jsModuleRuntime=[...e.jsModuleRuntime||[],...o.jsModuleRuntime||[]]),void 0!==o.wasmSymbols&&(o.wasmSymbols=[...e.wasmSymbols||[],...o.wasmSymbols||[]]),void 0!==o.wasmNative&&(o.wasmNative=[...e.wasmNative||[],...o.wasmNative||[]]),void 0!==o.icu&&(o.icu=[...e.icu||[],...o.icu||[]]),void 0!==o.satelliteResources&&(o.satelliteResources=function(e,t){if(e===t)return e;for(const o in t)e[o]=[...e[o]||[],...t[o]||[]];return e}(e.satelliteResources||{},o.satelliteResources||{})),void 0!==o.modulesAfterConfigLoaded&&(o.modulesAfterConfigLoaded=[...e.modulesAfterConfigLoaded||[],...o.modulesAfterConfigLoaded||[]]),void 0!==o.modulesAfterRuntimeReady&&(o.modulesAfterRuntimeReady=[...e.modulesAfterRuntimeReady||[],...o.modulesAfterRuntimeReady||[]]),void 0!==o.extensions&&(o.extensions={...e.extensions||{},...o.extensions||{}}),void 0!==o.vfs&&(o.vfs=[...e.vfs||[],...o.vfs||[]]),Object.assign(e,o)}function xe(){const e=Pe.config;if(e.environmentVariables=e.environmentVariables||{},e.runtimeOptions=e.runtimeOptions||[],e.resources=e.resources||{assembly:[],jsModuleNative:[],jsModuleWorker:[],jsModuleRuntime:[],wasmNative:[],vfs:[],satelliteResources:{}},e.assets){Pe.diagnosticTracing&&b("config.assets is deprecated, use config.resources instead");for(const t of e.assets){const o={};switch(t.behavior){case"assembly":o.assembly=[t];break;case"pdb":o.pdb=[t];break;case"resource":o.satelliteResources={},o.satelliteResources[t.culture]=[t];break;case"icu":o.icu=[t];break;case"symbols":o.wasmSymbols=[t];break;case"vfs":o.vfs=[t];break;case"dotnetwasm":o.wasmNative=[t];break;case"js-module-threads":o.jsModuleWorker=[t];break;case"js-module-runtime":o.jsModuleRuntime=[t];break;case"js-module-native":o.jsModuleNative=[t];break;case"js-module-diagnostics":o.jsModuleDiagnostics=[t];break;case"js-module-dotnet":break;default:throw new Error(`Unexpected behavior ${t.behavior} of asset ${t.name}`)}_e(e.resources,o)}}e.debugLevel,e.applicationEnvironment||(e.applicationEnvironment="Production"),e.applicationCulture&&(e.environmentVariables.LANG=`${e.applicationCulture}.UTF-8`),Ue.diagnosticTracing=Pe.diagnosticTracing=!!e.diagnosticTracing,Ue.waitForDebugger=e.waitForDebugger,Pe.maxParallelDownloads=e.maxParallelDownloads||Pe.maxParallelDownloads,Pe.enableDownloadRetry=void 0!==e.enableDownloadRetry?e.enableDownloadRetry:Pe.enableDownloadRetry}let je=!1;async function Re(e){var t;if(je)return void await Pe.afterConfigLoaded.promise;let o;try{if(e.configSrc||Pe.config&&0!==Object.keys(Pe.config).length&&(Pe.config.assets||Pe.config.resources)||(e.configSrc="dotnet.boot.js"),o=e.configSrc,je=!0,o&&(Pe.diagnosticTracing&&b("mono_wasm_load_config"),await async function(e){const t=e.configSrc,o=Pe.locateFile(t);let n=null;void 0!==Pe.loadBootResource&&(n=Pe.loadBootResource("manifest",t,o,"","manifest"));let r,i=null;if(n)if("string"==typeof n)n.includes(".json")?(i=await s(I(n)),r=await Ae(i)):r=(await import(I(n))).config;else{const e=await n;"function"==typeof e.json?(i=e,r=await Ae(i)):r=e.config}else o.includes(".json")?(i=await s(ce(o,"manifest")),r=await Ae(i)):r=(await import(ce(o,"manifest"))).config;function s(e){return Pe.fetch_like(e,{method:"GET",credentials:"include",cache:"no-cache"})}Pe.config.applicationEnvironment&&(r.applicationEnvironment=Pe.config.applicationEnvironment),ve(Pe.config,r)}(e)),xe(),await we(null===(t=Pe.config.resources)||void 0===t?void 0:t.modulesAfterConfigLoaded),await be("onRuntimeConfigLoaded",[Pe.config]),e.onConfigLoaded)try{await e.onConfigLoaded(Pe.config,Le),xe()}catch(e){throw _("onConfigLoaded() failed",e),e}xe(),Pe.afterConfigLoaded.promise_control.resolve(Pe.config)}catch(t){const n=`Failed to load config file ${o} ${t} ${null==t?void 0:t.stack}`;throw Pe.config=e.config=Object.assign(Pe.config,{message:n,error:t,isError:!0}),Xe(1,new Error(n)),t}}function Te(){return!!globalThis.navigator&&(Pe.isChromium||Pe.isFirefox)}async function Ae(e){const t=Pe.config,o=await e.json();t.applicationEnvironment||o.applicationEnvironment||(o.applicationEnvironment=e.headers.get("Blazor-Environment")||e.headers.get("DotNet-Environment")||void 0),o.environmentVariables||(o.environmentVariables={});const n=e.headers.get("DOTNET-MODIFIABLE-ASSEMBLIES");n&&(o.environmentVariables.DOTNET_MODIFIABLE_ASSEMBLIES=n);const r=e.headers.get("ASPNETCORE-BROWSER-TOOLS");return r&&(o.environmentVariables.__ASPNETCORE_BROWSER_TOOLS=r),o}"function"!=typeof importScripts||globalThis.onmessage||(globalThis.dotnetSidecar=!0);const Se="object"==typeof process&&"object"==typeof process.versions&&"string"==typeof process.versions.node,De="function"==typeof importScripts,Oe=De&&"undefined"!=typeof dotnetSidecar,Ce=De&&!Oe,ke="object"==typeof window||De&&!Se,Ie=!ke&&!Se;let Ue={},Pe={},Me={},Le={},Ne={},$e=!1;const ze={},We={config:ze},Fe={mono:{},binding:{},internal:Ne,module:We,loaderHelpers:Pe,runtimeHelpers:Ue,diagnosticHelpers:Me,api:Le};function Be(e,t){if(e)return;const o="Assert failed: "+("function"==typeof t?t():t),n=new Error(o);_(o,n),Ue.nativeAbort(n)}function Ve(){return void 0!==Pe.exitCode}function qe(){return Ue.runtimeReady&&!Ve()}function He(){Ve()&&Be(!1,`.NET runtime already exited with ${Pe.exitCode} ${Pe.exitReason}. You can use runtime.runMain() which doesn't exit the runtime.`),Ue.runtimeReady||Be(!1,".NET runtime didn't start yet. Please call dotnet.create() first.")}function Je(){ke&&(globalThis.addEventListener("unhandledrejection",et),globalThis.addEventListener("error",tt))}let Ze,Qe;function Ge(e){Qe&&Qe(e),Xe(e,Pe.exitReason)}function Ke(e){Ze&&Ze(e||Pe.exitReason),Xe(1,e||Pe.exitReason)}function Xe(t,o){var n,r;const i=o&&"object"==typeof o;t=i&&"number"==typeof o.status?o.status:void 0===t?-1:t;const s=i&&"string"==typeof o.message?o.message:""+o;(o=i?o:Ue.ExitStatus?function(e,t){const o=new Ue.ExitStatus(e);return o.message=t,o.toString=()=>t,o}(t,s):new Error("Exit with code "+t+" "+s)).status=t,o.message||(o.message=s);const a=""+(o.stack||(new Error).stack);try{Object.defineProperty(o,"stack",{get:()=>a})}catch(e){}const l=!!o.silent;if(o.silent=!0,Ve())Pe.diagnosticTracing&&b("mono_exit called after exit");else{try{We.onAbort==Ke&&(We.onAbort=Ze),We.onExit==Ge&&(We.onExit=Qe),ke&&(globalThis.removeEventListener("unhandledrejection",et),globalThis.removeEventListener("error",tt)),Ue.runtimeReady?(Ue.jiterpreter_dump_stats&&Ue.jiterpreter_dump_stats(!1),0===t&&(null===(n=Pe.config)||void 0===n?void 0:n.interopCleanupOnExit)&&Ue.forceDisposeProxies(!0,!0),e&&0!==t&&(null===(r=Pe.config)||void 0===r||r.dumpThreadsOnNonZeroExit)):(Pe.diagnosticTracing&&b(`abort_startup, reason: ${o}`),function(e){Pe.allDownloadsQueued.promise_control.reject(e),Pe.allDownloadsFinished.promise_control.reject(e),Pe.afterConfigLoaded.promise_control.reject(e),Pe.wasmCompilePromise.promise_control.reject(e),Pe.runtimeModuleLoaded.promise_control.reject(e),Ue.dotnetReady&&(Ue.dotnetReady.promise_control.reject(e),Ue.afterInstantiateWasm.promise_control.reject(e),Ue.beforePreInit.promise_control.reject(e),Ue.afterPreInit.promise_control.reject(e),Ue.afterPreRun.promise_control.reject(e),Ue.beforeOnRuntimeInitialized.promise_control.reject(e),Ue.afterOnRuntimeInitialized.promise_control.reject(e),Ue.afterPostRun.promise_control.reject(e))}(o))}catch(e){E("mono_exit A failed",e)}try{l||(function(e,t){if(0!==e&&t){const e=Ue.ExitStatus&&t instanceof Ue.ExitStatus?b:_;"string"==typeof t?e(t):(void 0===t.stack&&(t.stack=(new Error).stack+""),t.message?e(Ue.stringify_as_error_with_stack?Ue.stringify_as_error_with_stack(t.message+"\n"+t.stack):t.message+"\n"+t.stack):e(JSON.stringify(t)))}!Ce&&Pe.config&&(Pe.config.logExitCode?Pe.config.forwardConsoleLogsToWS?R("WASM EXIT "+e):v("WASM EXIT "+e):Pe.config.forwardConsoleLogsToWS&&R())}(t,o),function(e){if(ke&&!Ce&&Pe.config&&Pe.config.appendElementOnExit&&document){const t=document.createElement("label");t.id="tests_done",0!==e&&(t.style.background="red"),t.innerHTML=""+e,document.body.appendChild(t)}}(t))}catch(e){E("mono_exit B failed",e)}Pe.exitCode=t,Pe.exitReason||(Pe.exitReason=o),!Ce&&Ue.runtimeReady&&We.runtimeKeepalivePop()}if(Pe.config&&Pe.config.asyncFlushOnExit&&0===t)throw(async()=>{try{await async function(){try{const e=await import(/*! webpackIgnore: true */"process"),t=e=>new Promise(((t,o)=>{e.on("error",o),e.end("","utf8",t)})),o=t(e.stderr),n=t(e.stdout);let r;const i=new Promise((e=>{r=setTimeout((()=>e("timeout")),1e3)}));await Promise.race([Promise.all([n,o]),i]),clearTimeout(r)}catch(e){_(`flushing std* streams failed: ${e}`)}}()}finally{Ye(t,o)}})(),o;Ye(t,o)}function Ye(e,t){if(Ue.runtimeReady&&Ue.nativeExit)try{Ue.nativeExit(e)}catch(e){!Ue.ExitStatus||e instanceof Ue.ExitStatus||E("set_exit_code_and_quit_now failed: "+e.toString())}if(0!==e||!ke)throw Se&&Ne.process?Ne.process.exit(e):Ue.quit&&Ue.quit(e,t),t}function et(e){ot(e,e.reason,"rejection")}function tt(e){ot(e,e.error,"error")}function ot(e,t,o){e.preventDefault();try{t||(t=new Error("Unhandled "+o)),void 0===t.stack&&(t.stack=(new Error).stack),t.stack=t.stack+"",t.silent||(_("Unhandled error:",t),Xe(1,t))}catch(e){}}!function(e){if($e)throw new Error("Loader module already loaded");$e=!0,Ue=e.runtimeHelpers,Pe=e.loaderHelpers,Me=e.diagnosticHelpers,Le=e.api,Ne=e.internal,Object.assign(Le,{INTERNAL:Ne,invokeLibraryInitializers:be}),Object.assign(e.module,{config:ve(ze,{environmentVariables:{}})});const r={mono_wasm_bindings_is_ready:!1,config:e.module.config,diagnosticTracing:!1,nativeAbort:e=>{throw e||new Error("abort")},nativeExit:e=>{throw new Error("exit:"+e)}},l={gitHash:"b16286c2284fecf303dbc12a0bb152476d662e44",config:e.module.config,diagnosticTracing:!1,maxParallelDownloads:16,enableDownloadRetry:!0,_loaded_files:[],loadedFiles:[],loadedAssemblies:[],libraryInitializers:[],workerNextNumber:1,actual_downloaded_assets_count:0,actual_instantiated_assets_count:0,expected_downloaded_assets_count:0,expected_instantiated_assets_count:0,afterConfigLoaded:i(),allDownloadsQueued:i(),allDownloadsFinished:i(),wasmCompilePromise:i(),runtimeModuleLoaded:i(),loadingWorkers:i(),is_exited:Ve,is_runtime_running:qe,assert_runtime_running:He,mono_exit:Xe,createPromiseController:i,getPromiseController:s,assertIsControllablePromise:a,mono_download_assets:oe,resolve_single_asset_path:ee,setup_proxy_console:j,set_thread_prefix:w,installUnhandledErrorHandler:Je,retrieve_asset_download:ie,invokeLibraryInitializers:be,isDebuggingSupported:Te,exceptions:t,simd:n,relaxedSimd:o};Object.assign(Ue,r),Object.assign(Pe,l)}(Fe);let nt,rt,it,st=!1,at=!1;async function lt(e){if(!at){if(at=!0,ke&&Pe.config.forwardConsoleLogsToWS&&void 0!==globalThis.WebSocket&&j("main",globalThis.console,globalThis.location.origin),We||Be(!1,"Null moduleConfig"),Pe.config||Be(!1,"Null moduleConfig.config"),"function"==typeof e){const t=e(Fe.api);if(t.ready)throw new Error("Module.ready couldn't be redefined.");Object.assign(We,t),Ee(We,t)}else{if("object"!=typeof e)throw new Error("Can't use moduleFactory callback of createDotnetRuntime function.");Ee(We,e)}await async function(e){if(Se){const e=await import(/*! webpackIgnore: true */"process"),t=14;if(e.versions.node.split(".")[0]<t)throw new Error(`NodeJS at '${e.execPath}' has too low version '${e.versions.node}', please use at least ${t}. See also https://aka.ms/dotnet-wasm-features`)}const t=/*! webpackIgnore: true */import.meta.url,o=t.indexOf("?");var n;if(o>0&&(Pe.modulesUniqueQuery=t.substring(o)),Pe.scriptUrl=t.replace(/\\/g,"/").replace(/[?#].*/,""),Pe.scriptDirectory=(n=Pe.scriptUrl).slice(0,n.lastIndexOf("/"))+"/",Pe.locateFile=e=>"URL"in globalThis&&globalThis.URL!==C?new URL(e,Pe.scriptDirectory).toString():M(e)?e:Pe.scriptDirectory+e,Pe.fetch_like=k,Pe.out=console.log,Pe.err=console.error,Pe.onDownloadResourceProgress=e.onDownloadResourceProgress,ke&&globalThis.navigator){const e=globalThis.navigator,t=e.userAgentData&&e.userAgentData.brands;t&&t.length>0?Pe.isChromium=t.some((e=>"Google Chrome"===e.brand||"Microsoft Edge"===e.brand||"Chromium"===e.brand)):e.userAgent&&(Pe.isChromium=e.userAgent.includes("Chrome"),Pe.isFirefox=e.userAgent.includes("Firefox"))}Ne.require=Se?await import(/*! webpackIgnore: true */"module").then((e=>e.createRequire(/*! webpackIgnore: true */import.meta.url))):Promise.resolve((()=>{throw new Error("require not supported")})),void 0===globalThis.URL&&(globalThis.URL=C)}(We)}}async function ct(e){return await lt(e),Ze=We.onAbort,Qe=We.onExit,We.onAbort=Ke,We.onExit=Ge,We.ENVIRONMENT_IS_PTHREAD?async function(){(function(){const e=new MessageChannel,t=e.port1,o=e.port2;t.addEventListener("message",(e=>{var n,r;n=JSON.parse(e.data.config),r=JSON.parse(e.data.monoThreadInfo),st?Pe.diagnosticTracing&&b("mono config already received"):(ve(Pe.config,n),Ue.monoThreadInfo=r,xe(),Pe.diagnosticTracing&&b("mono config received"),st=!0,Pe.afterConfigLoaded.promise_control.resolve(Pe.config),ke&&n.forwardConsoleLogsToWS&&void 0!==globalThis.WebSocket&&Pe.setup_proxy_console("worker-idle",console,globalThis.location.origin)),t.close(),o.close()}),{once:!0}),t.start(),self.postMessage({[l]:{monoCmd:"preload",port:o}},[o])})(),await Pe.afterConfigLoaded.promise,function(){const e=Pe.config;e.assets||Be(!1,"config.assets must be defined");for(const t of e.assets)X(t),Q[t.behavior]&&z.push(t)}(),setTimeout((async()=>{try{await oe()}catch(e){Xe(1,e)}}),0);const e=dt(),t=await Promise.all(e);return await ut(t),We}():async function(){var e;await Re(We),re();const t=dt();(async function(){try{const e=ee("dotnetwasm");await se(e),e&&e.pendingDownloadInternal&&e.pendingDownloadInternal.response||Be(!1,"Can't load dotnet.native.wasm");const t=await e.pendingDownloadInternal.response,o=t.headers&&t.headers.get?t.headers.get("Content-Type"):void 0;let n;if("function"==typeof WebAssembly.compileStreaming&&"application/wasm"===o)n=await WebAssembly.compileStreaming(t);else{ke&&"application/wasm"!==o&&E('WebAssembly resource does not have the expected content type "application/wasm", so falling back to slower ArrayBuffer instantiation.');const e=await t.arrayBuffer();Pe.diagnosticTracing&&b("instantiate_wasm_module buffered"),n=Ie?await Promise.resolve(new WebAssembly.Module(e)):await WebAssembly.compile(e)}e.pendingDownloadInternal=null,e.pendingDownload=null,e.buffer=null,e.moduleExports=null,Pe.wasmCompilePromise.promise_control.resolve(n)}catch(e){Pe.wasmCompilePromise.promise_control.reject(e)}})(),setTimeout((async()=>{try{D(),await oe()}catch(e){Xe(1,e)}}),0);const o=await Promise.all(t);return await ut(o),await Ue.dotnetReady.promise,await we(null===(e=Pe.config.resources)||void 0===e?void 0:e.modulesAfterRuntimeReady),await be("onRuntimeReady",[Fe.api]),Le}()}function dt(){const e=ee("js-module-runtime"),t=ee("js-module-native");if(nt&&rt)return[nt,rt,it];"object"==typeof e.moduleExports?nt=e.moduleExports:(Pe.diagnosticTracing&&b(`Attempting to import '${e.resolvedUrl}' for ${e.name}`),nt=import(/*! webpackIgnore: true */e.resolvedUrl)),"object"==typeof t.moduleExports?rt=t.moduleExports:(Pe.diagnosticTracing&&b(`Attempting to import '${t.resolvedUrl}' for ${t.name}`),rt=import(/*! webpackIgnore: true */t.resolvedUrl));const o=Y("js-module-diagnostics");return o&&("object"==typeof o.moduleExports?it=o.moduleExports:(Pe.diagnosticTracing&&b(`Attempting to import '${o.resolvedUrl}' for ${o.name}`),it=import(/*! webpackIgnore: true */o.resolvedUrl))),[nt,rt,it]}async function ut(e){const{initializeExports:t,initializeReplacements:o,configureRuntimeStartup:n,configureEmscriptenStartup:r,configureWorkerStartup:i,setRuntimeGlobals:s,passEmscriptenInternals:a}=e[0],{default:l}=e[1],c=e[2];s(Fe),t(Fe),c&&c.setRuntimeGlobals(Fe),await n(We),Pe.runtimeModuleLoaded.promise_control.resolve(),l((e=>(Object.assign(We,{ready:e.ready,__dotnet_runtime:{initializeReplacements:o,configureEmscriptenStartup:r,configureWorkerStartup:i,passEmscriptenInternals:a}}),We))).catch((e=>{if(e.message&&e.message.toLowerCase().includes("out of memory"))throw new Error(".NET runtime has failed to start, because too much memory was requested. Please decrease the memory by adjusting EmccMaximumHeapSize. See also https://aka.ms/dotnet-wasm-features");throw e}))}const ft=new class{withModuleConfig(e){try{return Ee(We,e),this}catch(e){throw Xe(1,e),e}}withOnConfigLoaded(e){try{return Ee(We,{onConfigLoaded:e}),this}catch(e){throw Xe(1,e),e}}withConsoleForwarding(){try{return ve(ze,{forwardConsoleLogsToWS:!0}),this}catch(e){throw Xe(1,e),e}}withExitOnUnhandledError(){try{return ve(ze,{exitOnUnhandledError:!0}),Je(),this}catch(e){throw Xe(1,e),e}}withAsyncFlushOnExit(){try{return ve(ze,{asyncFlushOnExit:!0}),this}catch(e){throw Xe(1,e),e}}withExitCodeLogging(){try{return ve(ze,{logExitCode:!0}),this}catch(e){throw Xe(1,e),e}}withElementOnExit(){try{return ve(ze,{appendElementOnExit:!0}),this}catch(e){throw Xe(1,e),e}}withInteropCleanupOnExit(){try{return ve(ze,{interopCleanupOnExit:!0}),this}catch(e){throw Xe(1,e),e}}withDumpThreadsOnNonZeroExit(){try{return ve(ze,{dumpThreadsOnNonZeroExit:!0}),this}catch(e){throw Xe(1,e),e}}withWaitingForDebugger(e){try{return ve(ze,{waitForDebugger:e}),this}catch(e){throw Xe(1,e),e}}withInterpreterPgo(e,t){try{return ve(ze,{interpreterPgo:e,interpreterPgoSaveDelay:t}),ze.runtimeOptions?ze.runtimeOptions.push("--interp-pgo-recording"):ze.runtimeOptions=["--interp-pgo-recording"],this}catch(e){throw Xe(1,e),e}}withConfig(e){try{return ve(ze,e),this}catch(e){throw Xe(1,e),e}}withConfigSrc(e){try{return e&&"string"==typeof e||Be(!1,"must be file path or URL"),Ee(We,{configSrc:e}),this}catch(e){throw Xe(1,e),e}}withVirtualWorkingDirectory(e){try{return e&&"string"==typeof e||Be(!1,"must be directory path"),ve(ze,{virtualWorkingDirectory:e}),this}catch(e){throw Xe(1,e),e}}withEnvironmentVariable(e,t){try{const o={};return o[e]=t,ve(ze,{environmentVariables:o}),this}catch(e){throw Xe(1,e),e}}withEnvironmentVariables(e){try{return e&&"object"==typeof e||Be(!1,"must be dictionary object"),ve(ze,{environmentVariables:e}),this}catch(e){throw Xe(1,e),e}}withDiagnosticTracing(e){try{return"boolean"!=typeof e&&Be(!1,"must be boolean"),ve(ze,{diagnosticTracing:e}),this}catch(e){throw Xe(1,e),e}}withDebugging(e){try{return null!=e&&"number"==typeof e||Be(!1,"must be number"),ve(ze,{debugLevel:e}),this}catch(e){throw Xe(1,e),e}}withApplicationArguments(...e){try{return e&&Array.isArray(e)||Be(!1,"must be array of strings"),ve(ze,{applicationArguments:e}),this}catch(e){throw Xe(1,e),e}}withRuntimeOptions(e){try{return e&&Array.isArray(e)||Be(!1,"must be array of strings"),ze.runtimeOptions?ze.runtimeOptions.push(...e):ze.runtimeOptions=e,this}catch(e){throw Xe(1,e),e}}withMainAssembly(e){try{return ve(ze,{mainAssemblyName:e}),this}catch(e){throw Xe(1,e),e}}withApplicationArgumentsFromQuery(){try{if(!globalThis.window)throw new Error("Missing window to the query parameters from");if(void 0===globalThis.URLSearchParams)throw new Error("URLSearchParams is supported");const e=new URLSearchParams(globalThis.window.location.search).getAll("arg");return this.withApplicationArguments(...e)}catch(e){throw Xe(1,e),e}}withApplicationEnvironment(e){try{return ve(ze,{applicationEnvironment:e}),this}catch(e){throw Xe(1,e),e}}withApplicationCulture(e){try{return ve(ze,{applicationCulture:e}),this}catch(e){throw Xe(1,e),e}}withResourceLoader(e){try{return Pe.loadBootResource=e,this}catch(e){throw Xe(1,e),e}}async download(){try{await async function(){lt(We),await Re(We),re(),D(),oe(),await Pe.allDownloadsFinished.promise}()}catch(e){throw Xe(1,e),e}}async create(){try{return this.instance||(this.instance=await async function(){return await ct(We),Fe.api}()),this.instance}catch(e){throw Xe(1,e),e}}async run(){try{return We.config||Be(!1,"Null moduleConfig.config"),this.instance||await this.create(),this.instance.runMainAndExit()}catch(e){throw Xe(1,e),e}}},mt=Xe,gt=ct;Ie||"function"==typeof globalThis.URL||Be(!1,"This browser/engine doesn't support URL API. Please use a modern version. See also https://aka.ms/dotnet-wasm-features"),"function"!=typeof globalThis.BigInt64Array&&Be(!1,"This browser/engine doesn't support BigInt64Array API. Please use a modern version. See also https://aka.ms/dotnet-wasm-features"),ft.withConfig(/*json-start*/{
  "mainAssemblyName": "json-everything.net",
  "resources": {
    "hash": "sha256-wqImuP/uLYV0TNGTn5a4bIEmFvmu2vm9RsS+c7s4GFg=",
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
        "integrity": "sha256-2lvfACsds38yB7F9BvnIUtb0JBZIjimRTjlFpr4MLSw="
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
        "integrity": "sha256-vmPsNdFa4Bi+8IpGvCnd2p4jx8l5Ulg1xMh3ziOzMT8="
      },
      {
        "virtualPath": "System.Runtime.InteropServices.JavaScript.dll",
        "name": "System.Runtime.InteropServices.JavaScript.dll",
        "integrity": "sha256-UEvyM6IcackoYQTIUMYIzqo/TT2BsUQUYrotYuzkq8k="
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
        "virtualPath": "Bogus.dll",
        "name": "Bogus.dll",
        "integrity": "sha256-sz0/3YeMsmyiCOqVV28LbgomHyGrcvAMISZ08KZiMeA="
      },
      {
        "virtualPath": "ColorCode.dll",
        "name": "ColorCode.dll",
        "integrity": "sha256-JGrcn2j0wtkL4DMqzxgENQkz77cEYJrRUUlofxUJL2Y="
      },
      {
        "virtualPath": "Humanizer.dll",
        "name": "Humanizer.dll",
        "integrity": "sha256-J2ntgmW2kaVILHJQbtJ03FJEb6FkGZeowSxm9bkmPTk="
      },
      {
        "virtualPath": "IndexRange.dll",
        "name": "IndexRange.dll",
        "integrity": "sha256-E7RZqXJb865Csmg2YTIhRoRYlSv1n4SLgJwD7WAkf7E="
      },
      {
        "virtualPath": "Json.More.dll",
        "name": "Json.More.dll",
        "integrity": "sha256-Rctn9Gk/Aifcm6es1V+LzfI/xCWDX7fDYgECNIf9DFg="
      },
      {
        "virtualPath": "JsonE.Net.dll",
        "name": "JsonE.Net.dll",
        "integrity": "sha256-rTadkrrNYdT1yr9Z1yclIagVNsjYHLa01Dby/fd3cB4="
      },
      {
        "virtualPath": "JsonLogic.dll",
        "name": "JsonLogic.dll",
        "integrity": "sha256-d3wTa/aWjj/47adw1DFBf2DUYLR4nlw9jCcYby2HSiU="
      },
      {
        "virtualPath": "JsonPatch.Net.dll",
        "name": "JsonPatch.Net.dll",
        "integrity": "sha256-rtyzfDvZkSh7g4ojVXEArxHFY15za22XToWEM3wmqhg="
      },
      {
        "virtualPath": "JsonPath.Net.dll",
        "name": "JsonPath.Net.dll",
        "integrity": "sha256-wwy/onQDxt+c31JwA7uYHO2BPNatQJefYCzpWTxrGqA="
      },
      {
        "virtualPath": "JsonPointer.Net.dll",
        "name": "JsonPointer.Net.dll",
        "integrity": "sha256-CHA8pRMPMxtoskn1PJqM26HMQfI2/XsbXJZh8aMFi4A="
      },
      {
        "virtualPath": "JsonSchema.Net.ArrayExt.dll",
        "name": "JsonSchema.Net.ArrayExt.dll",
        "integrity": "sha256-1ekQtJ5Qd72g9ZNr0X/4b20UPZq+uys8YFXsEiPz3wA="
      },
      {
        "virtualPath": "JsonSchema.Net.Data.dll",
        "name": "JsonSchema.Net.Data.dll",
        "integrity": "sha256-osUhgG+qtnlx2R8Hjeo4PzlSl1VJwxWmq44Yv7y1Rjw="
      },
      {
        "virtualPath": "JsonSchema.Net.DataGeneration.dll",
        "name": "JsonSchema.Net.DataGeneration.dll",
        "integrity": "sha256-kzjcG8V/19BvNLwObluRTeozxAEtpYzsPFzTckM+5aI="
      },
      {
        "virtualPath": "JsonSchema.Net.Generation.DataAnnotations.dll",
        "name": "JsonSchema.Net.Generation.DataAnnotations.dll",
        "integrity": "sha256-yAnAd1v4f0k3IEhgqi4j+KGGRcYETbm0zqS4xZ4uAOQ="
      },
      {
        "virtualPath": "JsonSchema.Net.Generation.dll",
        "name": "JsonSchema.Net.Generation.dll",
        "integrity": "sha256-A7aG6J+c9oYzX+q07Jc40q8qhxe9Q1T8b+mFf7rPWXY="
      },
      {
        "virtualPath": "JsonSchema.Net.OpenApi.dll",
        "name": "JsonSchema.Net.OpenApi.dll",
        "integrity": "sha256-pmLrZXS+Gzjsvch5e46Lzk9C679jGMhvz9kP8HWMwmE="
      },
      {
        "virtualPath": "JsonSchema.Net.dll",
        "name": "JsonSchema.Net.dll",
        "integrity": "sha256-KsArnwrIdLbZtsYLLjj2AFSPVxWsYm32Js7oRqBI8X8="
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
        "integrity": "sha256-Q2XIN3CVipS4GddMRj2awuYtg7d5A1KzGB4OE0DFn0w="
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.Forms.dll",
        "name": "Microsoft.AspNetCore.Components.Forms.dll",
        "integrity": "sha256-stkBnmx+vIsI8Me3RZpzBcBCC3Rz3yJ8BPq8pq5tebw="
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.Web.dll",
        "name": "Microsoft.AspNetCore.Components.Web.dll",
        "integrity": "sha256-gw5JlgLgW2EfbZFYBcQsgcNGTQ9wvZt3HLZIOELJoxs="
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.WebAssembly.dll",
        "name": "Microsoft.AspNetCore.Components.WebAssembly.dll",
        "integrity": "sha256-Zh05kJQRAhAi1cItcOTdRaaBBfDIRqjiI7HHfovHYps="
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.dll",
        "name": "Microsoft.AspNetCore.Components.dll",
        "integrity": "sha256-FegMgFRt0V92h1K6WPYmiEZHohl/LhJM+xVyOR83raM="
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Metadata.dll",
        "name": "Microsoft.AspNetCore.Metadata.dll",
        "integrity": "sha256-Gbv+GTSFYPh8fHBx1EtL+hs0ppdtjcxqA4VZ3wNfU0o="
      },
      {
        "virtualPath": "Microsoft.Bcl.Memory.dll",
        "name": "Microsoft.Bcl.Memory.dll",
        "integrity": "sha256-2ESi4xt6CrwQkRYI4KgZUjqgVXdKW0bo4S3hTNWkSzI="
      },
      {
        "virtualPath": "Microsoft.CSharp.dll",
        "name": "Microsoft.CSharp.dll",
        "integrity": "sha256-BRcpggMW5V4aRq0RgsgxzUFD7ZrY9qQuRoI9BQhsCE0="
      },
      {
        "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.dll",
        "name": "Microsoft.CodeAnalysis.CSharp.Scripting.dll",
        "integrity": "sha256-zqzfUy37szHIkeBjCe7fdBlmY5K0F3Iyp1kYyz/Dl8c="
      },
      {
        "virtualPath": "Microsoft.CodeAnalysis.CSharp.dll",
        "name": "Microsoft.CodeAnalysis.CSharp.dll",
        "integrity": "sha256-VBQncKAOWUlC7okjcm6kkSYwPQxSOTKIbCQX62bcB/g="
      },
      {
        "virtualPath": "Microsoft.CodeAnalysis.Scripting.dll",
        "name": "Microsoft.CodeAnalysis.Scripting.dll",
        "integrity": "sha256-r3HmFyTqDn6v+NNxUYHPs/6QFFs0AmCalFewae5gpMo="
      },
      {
        "virtualPath": "Microsoft.CodeAnalysis.dll",
        "name": "Microsoft.CodeAnalysis.dll",
        "integrity": "sha256-rYV/HOf44mwnIq3nBfrD+QUCpHWQwUB13xMnIQ8Qse8="
      },
      {
        "virtualPath": "Microsoft.DotNet.HotReload.WebAssembly.Browser.dll",
        "name": "Microsoft.DotNet.HotReload.WebAssembly.Browser.dll",
        "integrity": "sha256-LeDSOFa94n0OdzM9MC2emRkae23kZdEUzvlmscgikyk="
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.Abstractions.dll",
        "name": "Microsoft.Extensions.Configuration.Abstractions.dll",
        "integrity": "sha256-5yuHVBKmE9AI+SRqddXrcXbcfvdaovIoygUiHUVZZm4="
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.Binder.dll",
        "name": "Microsoft.Extensions.Configuration.Binder.dll",
        "integrity": "sha256-mdo8B0xm4WvMN5ifOgrzBolbErpZT4qaC6PbSKCCd9Q="
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.FileExtensions.dll",
        "name": "Microsoft.Extensions.Configuration.FileExtensions.dll",
        "integrity": "sha256-bBbuF4fMcKlXyOs3KjUhfLGfOmlRARN6icdvUvadvYc="
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.Json.dll",
        "name": "Microsoft.Extensions.Configuration.Json.dll",
        "integrity": "sha256-sz7MhDs2/319OWCYTlMgpb1cjqPxyjJ3Iz8xRYJuojc="
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.dll",
        "name": "Microsoft.Extensions.Configuration.dll",
        "integrity": "sha256-4sgiDeI3yRPgvEpAxaqAA4l1d0vclAypqqhvvMGxNgM="
      },
      {
        "virtualPath": "Microsoft.Extensions.DependencyInjection.Abstractions.dll",
        "name": "Microsoft.Extensions.DependencyInjection.Abstractions.dll",
        "integrity": "sha256-hC/BmAKnYeScZbDpCyxovGhouYkUXaWsbKMLdm4mWPI="
      },
      {
        "virtualPath": "Microsoft.Extensions.DependencyInjection.dll",
        "name": "Microsoft.Extensions.DependencyInjection.dll",
        "integrity": "sha256-w7lv+tEC8b/Hm5zxw623pinWwfN1X5YGBQ7CrVWiLwY="
      },
      {
        "virtualPath": "Microsoft.Extensions.Diagnostics.Abstractions.dll",
        "name": "Microsoft.Extensions.Diagnostics.Abstractions.dll",
        "integrity": "sha256-RAY+T0vpA6FSqm4yc+hrHmRTYckt4qKtVZvicM8MKMM="
      },
      {
        "virtualPath": "Microsoft.Extensions.Diagnostics.dll",
        "name": "Microsoft.Extensions.Diagnostics.dll",
        "integrity": "sha256-RWrTIRSILAHwvOgi59KfMDYpoZb0ZSabEYOWSB1wNjQ="
      },
      {
        "virtualPath": "Microsoft.Extensions.FileProviders.Abstractions.dll",
        "name": "Microsoft.Extensions.FileProviders.Abstractions.dll",
        "integrity": "sha256-4bYO7fObvHVZxgmIFMNTrwjX6TzOF192c8YZU1JGd+I="
      },
      {
        "virtualPath": "Microsoft.Extensions.FileProviders.Physical.dll",
        "name": "Microsoft.Extensions.FileProviders.Physical.dll",
        "integrity": "sha256-cHWFfT2Sm3JDkX9jyTuZIDPfZudhdvtZddyAvJhXAak="
      },
      {
        "virtualPath": "Microsoft.Extensions.FileSystemGlobbing.dll",
        "name": "Microsoft.Extensions.FileSystemGlobbing.dll",
        "integrity": "sha256-0mBuTsjYZvUZZbl6UvN3bl4TpfeJVjw6ht1h0jOYRxU="
      },
      {
        "virtualPath": "Microsoft.Extensions.Http.dll",
        "name": "Microsoft.Extensions.Http.dll",
        "integrity": "sha256-87N6jQlQ26l//EtFhBuxt79cCmcy4puSqsOb4tSkik4="
      },
      {
        "virtualPath": "Microsoft.Extensions.Logging.Abstractions.dll",
        "name": "Microsoft.Extensions.Logging.Abstractions.dll",
        "integrity": "sha256-A+kWucX5Fxe2fHkDJdTl3S3r6B2OQDkogGuh1DDqcUc="
      },
      {
        "virtualPath": "Microsoft.Extensions.Logging.dll",
        "name": "Microsoft.Extensions.Logging.dll",
        "integrity": "sha256-YH6R9XUCaE410cdm6Ekm8uT501z7QXPAdk/m64qKaxE="
      },
      {
        "virtualPath": "Microsoft.Extensions.Options.ConfigurationExtensions.dll",
        "name": "Microsoft.Extensions.Options.ConfigurationExtensions.dll",
        "integrity": "sha256-rLaU/5LGawAewBnuBaGoiTnv2PBF6EK4rg6LhJDVQD4="
      },
      {
        "virtualPath": "Microsoft.Extensions.Options.dll",
        "name": "Microsoft.Extensions.Options.dll",
        "integrity": "sha256-b3pLwr5rNaHyjNYMJQ5UcarDHLKZXqfsc2XqXm+AZA4="
      },
      {
        "virtualPath": "Microsoft.Extensions.Primitives.dll",
        "name": "Microsoft.Extensions.Primitives.dll",
        "integrity": "sha256-5DZz/wvHYh+JPbHYAhKOQkLMxssMu+H6e114CQYoUU8="
      },
      {
        "virtualPath": "Microsoft.Extensions.Validation.dll",
        "name": "Microsoft.Extensions.Validation.dll",
        "integrity": "sha256-yPgm1SypfnsO5o7q9YUkTlDISGJbUUGupMIKJ31Ukf0="
      },
      {
        "virtualPath": "Microsoft.JSInterop.WebAssembly.dll",
        "name": "Microsoft.JSInterop.WebAssembly.dll",
        "integrity": "sha256-BZ2IY9ugTIj3Y7NXF4klgUgeOIITRspLgGViHE0vxFY="
      },
      {
        "virtualPath": "Microsoft.JSInterop.dll",
        "name": "Microsoft.JSInterop.dll",
        "integrity": "sha256-okiz21Yf0KTsZtFc506e5Fy9HHJp+NFk2VZRgbZtXfA="
      },
      {
        "virtualPath": "Microsoft.VisualBasic.Core.dll",
        "name": "Microsoft.VisualBasic.Core.dll",
        "integrity": "sha256-eFIvWSv38TPqeb+hb0wdDR+CGUVEVj5rFS7nX1XC51Q="
      },
      {
        "virtualPath": "Microsoft.VisualBasic.dll",
        "name": "Microsoft.VisualBasic.dll",
        "integrity": "sha256-hpJXl7gesSNdLbsG8O1VfvOfisgTdJ1W9YUzsccWG54="
      },
      {
        "virtualPath": "Microsoft.Win32.Primitives.dll",
        "name": "Microsoft.Win32.Primitives.dll",
        "integrity": "sha256-kt+ke4lWLoyEoLGQpWmzcZgYarpYEDJFX6SwjmKASkw="
      },
      {
        "virtualPath": "Microsoft.Win32.Registry.dll",
        "name": "Microsoft.Win32.Registry.dll",
        "integrity": "sha256-kpgyHtpewGiT09NDkYR/3hw9bOPoeQifcZUVNuEDay8="
      },
      {
        "virtualPath": "System.AppContext.dll",
        "name": "System.AppContext.dll",
        "integrity": "sha256-yZSVU2vvZuAqbNH1h2KQ9iykVCHkJDnGgL5VFymBI20="
      },
      {
        "virtualPath": "System.Buffers.dll",
        "name": "System.Buffers.dll",
        "integrity": "sha256-cETlWUOpxVFQdaVvsQFFd+457R3Pjstx1d3g3ptfp+4="
      },
      {
        "virtualPath": "System.Collections.Concurrent.dll",
        "name": "System.Collections.Concurrent.dll",
        "integrity": "sha256-00if4uTmfjX8ZTFVIrK32fl4PrwFwIGWQZDrYl6WMVM="
      },
      {
        "virtualPath": "System.Collections.Immutable.dll",
        "name": "System.Collections.Immutable.dll",
        "integrity": "sha256-68XuqqxNIdrqUkQ6etQqXiSRMxSB4gAQuhiblZgAgzA="
      },
      {
        "virtualPath": "System.Collections.NonGeneric.dll",
        "name": "System.Collections.NonGeneric.dll",
        "integrity": "sha256-jtOd4UtV6urGOD7uWoASbNTDr14iibu9lFZVGCtJD74="
      },
      {
        "virtualPath": "System.Collections.Specialized.dll",
        "name": "System.Collections.Specialized.dll",
        "integrity": "sha256-PPKBrrcv2yZFMC2PIEWy9m6XlZuRzoh060nj0EoRDyc="
      },
      {
        "virtualPath": "System.Collections.dll",
        "name": "System.Collections.dll",
        "integrity": "sha256-X906JIkbPlrtIbFW47s+Oaqx8TkaYWwbpM9PSs3DpoY="
      },
      {
        "virtualPath": "System.ComponentModel.Annotations.dll",
        "name": "System.ComponentModel.Annotations.dll",
        "integrity": "sha256-JLfajQFpcKSgENyt4blwCu5pcm528CGGogzplPbE8ww="
      },
      {
        "virtualPath": "System.ComponentModel.DataAnnotations.dll",
        "name": "System.ComponentModel.DataAnnotations.dll",
        "integrity": "sha256-CTGlBrQ1OMtliTNNfBkeziEfHcUyCc0y2AC9CTUehNw="
      },
      {
        "virtualPath": "System.ComponentModel.EventBasedAsync.dll",
        "name": "System.ComponentModel.EventBasedAsync.dll",
        "integrity": "sha256-ikLjrWebqJOlkGtpx9lJPAtguefn7/FVnOQQmmWGT6c="
      },
      {
        "virtualPath": "System.ComponentModel.Primitives.dll",
        "name": "System.ComponentModel.Primitives.dll",
        "integrity": "sha256-P8ECUJw9B6tWiG7INbNHUL5S8iHQG2AvM6eYA312CHo="
      },
      {
        "virtualPath": "System.ComponentModel.TypeConverter.dll",
        "name": "System.ComponentModel.TypeConverter.dll",
        "integrity": "sha256-GzEaP5KeFdWpHSbmLMj8CUN43z2FiYbeGx5GfdNegcA="
      },
      {
        "virtualPath": "System.ComponentModel.dll",
        "name": "System.ComponentModel.dll",
        "integrity": "sha256-Dk+HxmEyhZPTraNEht8ao9UtZ1wnV+fg5cUxB84oU6Y="
      },
      {
        "virtualPath": "System.Configuration.dll",
        "name": "System.Configuration.dll",
        "integrity": "sha256-VEPxJE5gZJNRT/ZEc7+TrQ8wm5UPi1Y3K3Liy01C0SQ="
      },
      {
        "virtualPath": "System.Console.dll",
        "name": "System.Console.dll",
        "integrity": "sha256-1JAiD9NE+HQO7eNsZscTbXZC5yk0AjB08qKfilk+EHc="
      },
      {
        "virtualPath": "System.Core.dll",
        "name": "System.Core.dll",
        "integrity": "sha256-PAd/wnV7xc/NCg14Kjo3BgYDRkoY55PL68iJDigHa38="
      },
      {
        "virtualPath": "System.Data.Common.dll",
        "name": "System.Data.Common.dll",
        "integrity": "sha256-8rb0cPq0aQAuDceF63BtCpB7fJqqNQZ4/2YhFg8HGwc="
      },
      {
        "virtualPath": "System.Data.DataSetExtensions.dll",
        "name": "System.Data.DataSetExtensions.dll",
        "integrity": "sha256-2EiABwRJI8sfmQcD8ZSa5eSgEc+e2/bkSlZVWNmsXqo="
      },
      {
        "virtualPath": "System.Data.dll",
        "name": "System.Data.dll",
        "integrity": "sha256-F1WG07VIpkSF4cDEXBnFumm0XiTE/7WlsAiIwWuOu2o="
      },
      {
        "virtualPath": "System.Diagnostics.Contracts.dll",
        "name": "System.Diagnostics.Contracts.dll",
        "integrity": "sha256-anq8NC+fXZ1xadaoNoFiKQWwdNOqZ1oqKiAy/nM2hJA="
      },
      {
        "virtualPath": "System.Diagnostics.Debug.dll",
        "name": "System.Diagnostics.Debug.dll",
        "integrity": "sha256-diG7YVtCfWtOIn0MMFxPFr9AkBl8h0iPy5G+pF8f8CI="
      },
      {
        "virtualPath": "System.Diagnostics.DiagnosticSource.dll",
        "name": "System.Diagnostics.DiagnosticSource.dll",
        "integrity": "sha256-xGLS4QLVMtu2P4Ji1qUErpXosmlXiV3lweIoQQWpLdA="
      },
      {
        "virtualPath": "System.Diagnostics.FileVersionInfo.dll",
        "name": "System.Diagnostics.FileVersionInfo.dll",
        "integrity": "sha256-KlcEoXnYEfcj4qo2lgY5s3Lq767ifuuQRgtDDM/I7oY="
      },
      {
        "virtualPath": "System.Diagnostics.Process.dll",
        "name": "System.Diagnostics.Process.dll",
        "integrity": "sha256-7x8/u7YdEcMnLMaKLWtLf5Zl7LHh19EIAwlfHeKLYTs="
      },
      {
        "virtualPath": "System.Diagnostics.StackTrace.dll",
        "name": "System.Diagnostics.StackTrace.dll",
        "integrity": "sha256-QEu0u4QLVyi95NyiSEGBF+LcVFf0vRNW5El3P/DS9zE="
      },
      {
        "virtualPath": "System.Diagnostics.TextWriterTraceListener.dll",
        "name": "System.Diagnostics.TextWriterTraceListener.dll",
        "integrity": "sha256-BV6wPaKgKq8vvDG3k2yO9S2Y+NCY18l+RPszmjFaH94="
      },
      {
        "virtualPath": "System.Diagnostics.Tools.dll",
        "name": "System.Diagnostics.Tools.dll",
        "integrity": "sha256-AirGCOhcoEuOez8vKcH5KQoS2FkxIAfdBudu5bs5Sys="
      },
      {
        "virtualPath": "System.Diagnostics.TraceSource.dll",
        "name": "System.Diagnostics.TraceSource.dll",
        "integrity": "sha256-IdWH9m7c8+ZRfXS/Br0RywMHZQnCkrxJoeghuBK+O+Q="
      },
      {
        "virtualPath": "System.Diagnostics.Tracing.dll",
        "name": "System.Diagnostics.Tracing.dll",
        "integrity": "sha256-+pLHmloLe7MI2cn09ABU+2/jXSgawTcMUgP3VwQFgYY="
      },
      {
        "virtualPath": "System.Drawing.Primitives.dll",
        "name": "System.Drawing.Primitives.dll",
        "integrity": "sha256-xj3DSeSXw3ZpSWi2J55ABPNkHKpi6WEShxsjTjeXVsA="
      },
      {
        "virtualPath": "System.Drawing.dll",
        "name": "System.Drawing.dll",
        "integrity": "sha256-spkfNcT1pNGPBh+7JDMD2mLBO+7dJuudM61XF6N4muQ="
      },
      {
        "virtualPath": "System.Dynamic.Runtime.dll",
        "name": "System.Dynamic.Runtime.dll",
        "integrity": "sha256-XRr3mQW0saPqrNS8bD5ZXUEHmqmdcr4e4rToJg/i+7E="
      },
      {
        "virtualPath": "System.Formats.Asn1.dll",
        "name": "System.Formats.Asn1.dll",
        "integrity": "sha256-1jRCsIXrSPPK4v72O5iRrZ4/PWeaXp9PnoOzGL/ytF8="
      },
      {
        "virtualPath": "System.Formats.Tar.dll",
        "name": "System.Formats.Tar.dll",
        "integrity": "sha256-3uM0T/0YNY/+fLqu9MbR3VIDCiFTDb1ktLkBfIZIf0w="
      },
      {
        "virtualPath": "System.Globalization.Calendars.dll",
        "name": "System.Globalization.Calendars.dll",
        "integrity": "sha256-TO9YcFh/cG2jVAsMR5zBxyqcm0ZEXDDpL5Qj5zE7VTw="
      },
      {
        "virtualPath": "System.Globalization.Extensions.dll",
        "name": "System.Globalization.Extensions.dll",
        "integrity": "sha256-oupskC8Ma+/QpubcF7cQmyLiVcdXl8DWJOD1Q7np7Nk="
      },
      {
        "virtualPath": "System.Globalization.dll",
        "name": "System.Globalization.dll",
        "integrity": "sha256-dcH7nt5dK060sz3aN08zBx4CjERVkfesnPy/wBKDz+E="
      },
      {
        "virtualPath": "System.IO.Compression.Brotli.dll",
        "name": "System.IO.Compression.Brotli.dll",
        "integrity": "sha256-9qPiWOxRJ0HOh1ugIPBjj3Cs3kmUil3nXd2vysdr3No="
      },
      {
        "virtualPath": "System.IO.Compression.FileSystem.dll",
        "name": "System.IO.Compression.FileSystem.dll",
        "integrity": "sha256-uMR4OANCa5q/A8Xwy4xx2NP3VdYv2foVCog1LjMDPJc="
      },
      {
        "virtualPath": "System.IO.Compression.ZipFile.dll",
        "name": "System.IO.Compression.ZipFile.dll",
        "integrity": "sha256-uGJJ/fYLXAdpoGB1z5tFpYv+uaJMwb5Z1JNE5l3NuOU="
      },
      {
        "virtualPath": "System.IO.Compression.dll",
        "name": "System.IO.Compression.dll",
        "integrity": "sha256-Locrkl/ii+FeYsL8QgbrUKnD3/NWqNTewg9aTMdjm3M="
      },
      {
        "virtualPath": "System.IO.FileSystem.AccessControl.dll",
        "name": "System.IO.FileSystem.AccessControl.dll",
        "integrity": "sha256-GzixxMMAOwEsAJhxmlcHfgtxSNwFbXnwdz+Yt+7+8K8="
      },
      {
        "virtualPath": "System.IO.FileSystem.DriveInfo.dll",
        "name": "System.IO.FileSystem.DriveInfo.dll",
        "integrity": "sha256-Z0I+skCU7AHQmcZt+DMPv0qA1UXnhPUNWOKh6HrcijA="
      },
      {
        "virtualPath": "System.IO.FileSystem.Primitives.dll",
        "name": "System.IO.FileSystem.Primitives.dll",
        "integrity": "sha256-KUMI5hTN2xAtgbJT+ftgDzCT5REiiIfuw4/u19Y9Vj0="
      },
      {
        "virtualPath": "System.IO.FileSystem.Watcher.dll",
        "name": "System.IO.FileSystem.Watcher.dll",
        "integrity": "sha256-DR/g+VgrEmRxDysF84S+NqH6ODqyW11JATeXoJupEac="
      },
      {
        "virtualPath": "System.IO.FileSystem.dll",
        "name": "System.IO.FileSystem.dll",
        "integrity": "sha256-zUkEAyvzd7/3In3NhBWxnqjmcuCew29oaVPcTcXbBOI="
      },
      {
        "virtualPath": "System.IO.IsolatedStorage.dll",
        "name": "System.IO.IsolatedStorage.dll",
        "integrity": "sha256-nCWezV4+YJm8G7AeMjE639NDAAonOYQb69xreFm6Q3U="
      },
      {
        "virtualPath": "System.IO.MemoryMappedFiles.dll",
        "name": "System.IO.MemoryMappedFiles.dll",
        "integrity": "sha256-LYWCVq18maE1JTwiJgYfEQ4Ml2GQQi8bX91dMFJ8HO4="
      },
      {
        "virtualPath": "System.IO.Pipelines.dll",
        "name": "System.IO.Pipelines.dll",
        "integrity": "sha256-QJKTe1aSnQVnuZXsY0t/iUepsC4LBXvMUSkERpwK0jQ="
      },
      {
        "virtualPath": "System.IO.Pipes.AccessControl.dll",
        "name": "System.IO.Pipes.AccessControl.dll",
        "integrity": "sha256-f28LVRDtM7pSVR3CyBIIU3uGocGkJKLYaLNEOGnuEZA="
      },
      {
        "virtualPath": "System.IO.Pipes.dll",
        "name": "System.IO.Pipes.dll",
        "integrity": "sha256-1s5D3koEh7tF6GtOumBDAed945/hEl24A8pqdyLcVnU="
      },
      {
        "virtualPath": "System.IO.UnmanagedMemoryStream.dll",
        "name": "System.IO.UnmanagedMemoryStream.dll",
        "integrity": "sha256-PoZLDztDMdl3L9UgOLu6Mf1vNsmClPtLYM3RrwD+Ylc="
      },
      {
        "virtualPath": "System.IO.dll",
        "name": "System.IO.dll",
        "integrity": "sha256-d0mbvu969sYOdMSKrEMvfUHQqrbMbh6j/7bzuU0Hrtw="
      },
      {
        "virtualPath": "System.Linq.AsyncEnumerable.dll",
        "name": "System.Linq.AsyncEnumerable.dll",
        "integrity": "sha256-+iwswcSDb0tdSQUFdmFg+1oc0UtOI7G8RVwEOFQ/9VQ="
      },
      {
        "virtualPath": "System.Linq.Expressions.dll",
        "name": "System.Linq.Expressions.dll",
        "integrity": "sha256-mPkic4zrjxyRnUsoFhzyqnj16UUiJ8PXtf3YxjTXpIY="
      },
      {
        "virtualPath": "System.Linq.Parallel.dll",
        "name": "System.Linq.Parallel.dll",
        "integrity": "sha256-Pa77PJ8rq/Mm7Fud0pCXavWyPm4YdQiPN6fPheOZZXE="
      },
      {
        "virtualPath": "System.Linq.Queryable.dll",
        "name": "System.Linq.Queryable.dll",
        "integrity": "sha256-bH1pcOAWOR1ytgDNTw8TV+OJuOwGuOTfqvt+oAHIrtM="
      },
      {
        "virtualPath": "System.Linq.dll",
        "name": "System.Linq.dll",
        "integrity": "sha256-jw0x7/E5nimPXpQs3yXdbq9f9Yw8ZzGy6POa0QMtjLA="
      },
      {
        "virtualPath": "System.Memory.dll",
        "name": "System.Memory.dll",
        "integrity": "sha256-MuN6EoW3FM/wf15nm5auxbUI5xSsClcEK4MSwdYv9tw="
      },
      {
        "virtualPath": "System.Net.Http.Json.dll",
        "name": "System.Net.Http.Json.dll",
        "integrity": "sha256-bJks8gnU75c3SYXJgYaof3ZuY4PLy8gmKzxFFOocr7Q="
      },
      {
        "virtualPath": "System.Net.Http.dll",
        "name": "System.Net.Http.dll",
        "integrity": "sha256-mu7JghYR5FpsVjj+jEvTFnSXOI21i3XH1K/TfToQObw="
      },
      {
        "virtualPath": "System.Net.HttpListener.dll",
        "name": "System.Net.HttpListener.dll",
        "integrity": "sha256-YvpP7jawdRmh/qMX4ws/m9tq+dgNrgAi8ybbl6eSP30="
      },
      {
        "virtualPath": "System.Net.Mail.dll",
        "name": "System.Net.Mail.dll",
        "integrity": "sha256-ZCVsUMvNoBotYNs1uVq2cNxInYQfJgHYzemwC1TebYY="
      },
      {
        "virtualPath": "System.Net.NameResolution.dll",
        "name": "System.Net.NameResolution.dll",
        "integrity": "sha256-b6BCj5AUKiYcHHt44h/aAAV0kD1/CeUfgD+VduaDpzU="
      },
      {
        "virtualPath": "System.Net.NetworkInformation.dll",
        "name": "System.Net.NetworkInformation.dll",
        "integrity": "sha256-QpWkoA7NfgiHcSvvnzh/acfGbHf3H6xp44T0q8eE9ZI="
      },
      {
        "virtualPath": "System.Net.Ping.dll",
        "name": "System.Net.Ping.dll",
        "integrity": "sha256-SYvrl39N2o+p/8TxHYT94Na70g5TSGVesbjmhyd/KZ4="
      },
      {
        "virtualPath": "System.Net.Primitives.dll",
        "name": "System.Net.Primitives.dll",
        "integrity": "sha256-Bp+m/7xYkZoHjyOK1hLCfZ55ubQfhD10BPF0bro4OSI="
      },
      {
        "virtualPath": "System.Net.Quic.dll",
        "name": "System.Net.Quic.dll",
        "integrity": "sha256-RWX6MS7hLRFz6610aNk2QssRTkG6Z1IRRKMGaRGZEKE="
      },
      {
        "virtualPath": "System.Net.Requests.dll",
        "name": "System.Net.Requests.dll",
        "integrity": "sha256-iuaWJAbm0jS6P2aVv2uy70YxqAUr/3oyke32Kxh7H2E="
      },
      {
        "virtualPath": "System.Net.Security.dll",
        "name": "System.Net.Security.dll",
        "integrity": "sha256-IuVGaq/tFibuYAqhvKIppPQ36HtcqMfcUVFw+XVwG8Y="
      },
      {
        "virtualPath": "System.Net.ServerSentEvents.dll",
        "name": "System.Net.ServerSentEvents.dll",
        "integrity": "sha256-ofW5MF0ZH6glLjhd6dcELUSDyjgjiz7Vo+74YcMLBBI="
      },
      {
        "virtualPath": "System.Net.ServicePoint.dll",
        "name": "System.Net.ServicePoint.dll",
        "integrity": "sha256-olEDbdOxwBrI2oWxhEhcuV9kooWkmmhNcpdrD/Uw7Lo="
      },
      {
        "virtualPath": "System.Net.Sockets.dll",
        "name": "System.Net.Sockets.dll",
        "integrity": "sha256-CIBtA3f3O1D2hY9Rg/qLGNjCpynOGuuk1iTsLVE4K5c="
      },
      {
        "virtualPath": "System.Net.WebClient.dll",
        "name": "System.Net.WebClient.dll",
        "integrity": "sha256-RGMO06cyukipIrNR/hPlZFQhIK6yil9Wd+EINjBg1NY="
      },
      {
        "virtualPath": "System.Net.WebHeaderCollection.dll",
        "name": "System.Net.WebHeaderCollection.dll",
        "integrity": "sha256-6Tpfgkgg3l8cyW+53ZqJSsve6py4y0O0IPgFy2QA2vU="
      },
      {
        "virtualPath": "System.Net.WebProxy.dll",
        "name": "System.Net.WebProxy.dll",
        "integrity": "sha256-7a6nYwp/STAvdbUiNji/gzGXxAQrO4w1zgkrrgdNEs8="
      },
      {
        "virtualPath": "System.Net.WebSockets.Client.dll",
        "name": "System.Net.WebSockets.Client.dll",
        "integrity": "sha256-rReUgAxqgb2qqAQTS84YL+Sbvxq4xamzf8DBmMXWa5U="
      },
      {
        "virtualPath": "System.Net.WebSockets.dll",
        "name": "System.Net.WebSockets.dll",
        "integrity": "sha256-blnFZVFRpqudMElt3PuttgJc29YaOqCjIFJd0SzhEGg="
      },
      {
        "virtualPath": "System.Net.dll",
        "name": "System.Net.dll",
        "integrity": "sha256-dE0xksGeuJf9kM8FBWVXpxKOT5lU6kLoV8MPSdmA8o4="
      },
      {
        "virtualPath": "System.Numerics.Vectors.dll",
        "name": "System.Numerics.Vectors.dll",
        "integrity": "sha256-MUBMO8Wt+knYXqYSTmHU+jxNDeQCX2zMTid0H2ZuYNU="
      },
      {
        "virtualPath": "System.Numerics.dll",
        "name": "System.Numerics.dll",
        "integrity": "sha256-cwixndkxVLa+gPJYG5Y/szqa4Znimg5Qa6QCUvTboe8="
      },
      {
        "virtualPath": "System.ObjectModel.dll",
        "name": "System.ObjectModel.dll",
        "integrity": "sha256-CltDF1XUPZM3JRm9ilnNzCTaPvxOpfihGQcoTOI6BWY="
      },
      {
        "virtualPath": "System.Private.DataContractSerialization.dll",
        "name": "System.Private.DataContractSerialization.dll",
        "integrity": "sha256-bFd3LkZqiW/MgNnvBzZztMk+52Nx0FyrsGbm+oeNHy8="
      },
      {
        "virtualPath": "System.Private.Uri.dll",
        "name": "System.Private.Uri.dll",
        "integrity": "sha256-HzOb8JgOFN8EFD5k0aymwM5ABR3WwqNQST6BwNJtJEE="
      },
      {
        "virtualPath": "System.Private.Xml.Linq.dll",
        "name": "System.Private.Xml.Linq.dll",
        "integrity": "sha256-Fy38gyMz18whstHh7D59STPxPHizhJBYJC3EB2dlLmg="
      },
      {
        "virtualPath": "System.Private.Xml.dll",
        "name": "System.Private.Xml.dll",
        "integrity": "sha256-pitMMuO/r+0YNuqMfv9aM+dOnKI7paH8XffSjv0oVMk="
      },
      {
        "virtualPath": "System.Reflection.DispatchProxy.dll",
        "name": "System.Reflection.DispatchProxy.dll",
        "integrity": "sha256-8vrmliXRh8LJbr6Sh9STbQ+0Vogm2omjA44STS2c65Y="
      },
      {
        "virtualPath": "System.Reflection.Emit.ILGeneration.dll",
        "name": "System.Reflection.Emit.ILGeneration.dll",
        "integrity": "sha256-IZv9H2ksTNw1IdX/y8tv2WmAGg0KXXqu/kVKqd03KJI="
      },
      {
        "virtualPath": "System.Reflection.Emit.Lightweight.dll",
        "name": "System.Reflection.Emit.Lightweight.dll",
        "integrity": "sha256-s+9+1y/b2ZHpEb4kioMjFE1C2ejDru2NCmCB6C9cfok="
      },
      {
        "virtualPath": "System.Reflection.Emit.dll",
        "name": "System.Reflection.Emit.dll",
        "integrity": "sha256-CRlwj63+hIVENbx0JGNBwARe/X6Sv7cARyW6Kpmr0w0="
      },
      {
        "virtualPath": "System.Reflection.Extensions.dll",
        "name": "System.Reflection.Extensions.dll",
        "integrity": "sha256-h/OLZesugoPOI267MgNuLBfK7JZpLPi9FTbtWPGN+Ek="
      },
      {
        "virtualPath": "System.Reflection.Metadata.dll",
        "name": "System.Reflection.Metadata.dll",
        "integrity": "sha256-BFNeDFXgPK8efvWk0oODv9eNVHQ4czARFLhabNMzbwg="
      },
      {
        "virtualPath": "System.Reflection.Primitives.dll",
        "name": "System.Reflection.Primitives.dll",
        "integrity": "sha256-NRAaFqqkDP8sm7OqcDSEH4WNY6KIhiTbJg53fGLwfmc="
      },
      {
        "virtualPath": "System.Reflection.TypeExtensions.dll",
        "name": "System.Reflection.TypeExtensions.dll",
        "integrity": "sha256-B+dJKznsGL06/onWEAgQQIKsIzoOBSplbnCdapU8jVQ="
      },
      {
        "virtualPath": "System.Reflection.dll",
        "name": "System.Reflection.dll",
        "integrity": "sha256-Jxb9J3F+X1K3uPdfu3iI8LqijpFJeFUL55ClHvu80n4="
      },
      {
        "virtualPath": "System.Resources.Reader.dll",
        "name": "System.Resources.Reader.dll",
        "integrity": "sha256-5LFF5PgogUYzv9/ZKxnz67pQPkpR1XyqO2qNvzazark="
      },
      {
        "virtualPath": "System.Resources.ResourceManager.dll",
        "name": "System.Resources.ResourceManager.dll",
        "integrity": "sha256-CI+esSDi7ckM+hLyxMalZdelAPQ5yARS8Mkhyevih7I="
      },
      {
        "virtualPath": "System.Resources.Writer.dll",
        "name": "System.Resources.Writer.dll",
        "integrity": "sha256-V73DwOe8ddt3LBU6q49Gh6ackSA5G0XZ83IKhf47bS4="
      },
      {
        "virtualPath": "System.Runtime.CompilerServices.Unsafe.dll",
        "name": "System.Runtime.CompilerServices.Unsafe.dll",
        "integrity": "sha256-pHn6OAgcbVZLcCU/3MV+Fj8wuUnJnr7YG9zH/amOA/g="
      },
      {
        "virtualPath": "System.Runtime.CompilerServices.VisualC.dll",
        "name": "System.Runtime.CompilerServices.VisualC.dll",
        "integrity": "sha256-vDOhVtIsyMCbRhvwk/U252stgedm6hTOzWykkk7/Y/Q="
      },
      {
        "virtualPath": "System.Runtime.Extensions.dll",
        "name": "System.Runtime.Extensions.dll",
        "integrity": "sha256-UlxhUnUxv1Ra+VORoy8vUhuSQpo2jSFQfJ/jeVUdG7c="
      },
      {
        "virtualPath": "System.Runtime.Handles.dll",
        "name": "System.Runtime.Handles.dll",
        "integrity": "sha256-x7M9b1mMKOwFEi97alKGqlv+QCoj0cbLRyqN34DKqNA="
      },
      {
        "virtualPath": "System.Runtime.InteropServices.RuntimeInformation.dll",
        "name": "System.Runtime.InteropServices.RuntimeInformation.dll",
        "integrity": "sha256-pahwA0qw1pkF+LElX8tI8dUhQSbGfwFWbdOLcGMXLus="
      },
      {
        "virtualPath": "System.Runtime.InteropServices.dll",
        "name": "System.Runtime.InteropServices.dll",
        "integrity": "sha256-NQI64PRRltqZaVQ6bIUxnJhXSW0Yuhhp3ywkk1xGGQ8="
      },
      {
        "virtualPath": "System.Runtime.Intrinsics.dll",
        "name": "System.Runtime.Intrinsics.dll",
        "integrity": "sha256-oqBTZVfp98XOOR+zyOJsp0Q1BtCxaaJCj8G3XQIzmn8="
      },
      {
        "virtualPath": "System.Runtime.Loader.dll",
        "name": "System.Runtime.Loader.dll",
        "integrity": "sha256-9uqXXqG77WVf0TlSkNKLCHbtXyaCzOma2FlcxB+q39U="
      },
      {
        "virtualPath": "System.Runtime.Numerics.dll",
        "name": "System.Runtime.Numerics.dll",
        "integrity": "sha256-825+ldE6LeGTEnahElMdY9KJ3N+qJKIivtsf8eGiDXs="
      },
      {
        "virtualPath": "System.Runtime.Serialization.Formatters.dll",
        "name": "System.Runtime.Serialization.Formatters.dll",
        "integrity": "sha256-hcquVQOwkgJdbV34GM8kCaJhpbRUV+b12yfm1+EB7oo="
      },
      {
        "virtualPath": "System.Runtime.Serialization.Json.dll",
        "name": "System.Runtime.Serialization.Json.dll",
        "integrity": "sha256-rLUgdMdlh6xtYGZxY3SYwUgVcsCf+G9v/SXn6LhM/B4="
      },
      {
        "virtualPath": "System.Runtime.Serialization.Primitives.dll",
        "name": "System.Runtime.Serialization.Primitives.dll",
        "integrity": "sha256-N4wy/FOnsTCrJZqFQS++U4llpPaFHjJdy/5Ieeks0mI="
      },
      {
        "virtualPath": "System.Runtime.Serialization.Xml.dll",
        "name": "System.Runtime.Serialization.Xml.dll",
        "integrity": "sha256-w5uwA7ySCoCHGZv7FKQRhimqgyRXjQ3FQUm81KWs5ps="
      },
      {
        "virtualPath": "System.Runtime.Serialization.dll",
        "name": "System.Runtime.Serialization.dll",
        "integrity": "sha256-QePgmt3lwIMkIP93WwKjudh+x95bHjgvQtXoZi7euRU="
      },
      {
        "virtualPath": "System.Runtime.dll",
        "name": "System.Runtime.dll",
        "integrity": "sha256-PjHsR38J3KnDPcUgSAvARlfVu3x68e7Cx6WfZYCduLQ="
      },
      {
        "virtualPath": "System.Security.AccessControl.dll",
        "name": "System.Security.AccessControl.dll",
        "integrity": "sha256-zQViE0XH43sETfTr4AmNIAPS91GQ+2tHwlUVhIrl3Lo="
      },
      {
        "virtualPath": "System.Security.Claims.dll",
        "name": "System.Security.Claims.dll",
        "integrity": "sha256-Wcc/5As5VMu0QKRProvxAPeuKvBs5UWKK4LlJdc5NeQ="
      },
      {
        "virtualPath": "System.Security.Cryptography.Algorithms.dll",
        "name": "System.Security.Cryptography.Algorithms.dll",
        "integrity": "sha256-vrLTD5MHb658S62TIjcGKdMhSK1L8VsJObWDkvjfNGU="
      },
      {
        "virtualPath": "System.Security.Cryptography.Cng.dll",
        "name": "System.Security.Cryptography.Cng.dll",
        "integrity": "sha256-y/Mlmbv75nprAsJE+UB5dVuF0yBsCdrJouhs8KqIt6Q="
      },
      {
        "virtualPath": "System.Security.Cryptography.Csp.dll",
        "name": "System.Security.Cryptography.Csp.dll",
        "integrity": "sha256-+Nsy0nBR0oKRjChMZvBevldkT8VceLEGq5pfAm5RHLs="
      },
      {
        "virtualPath": "System.Security.Cryptography.Encoding.dll",
        "name": "System.Security.Cryptography.Encoding.dll",
        "integrity": "sha256-tw8DmbzdOxdyIxhdJd/G1Rozzrdj6f16jy5ChdZm8SM="
      },
      {
        "virtualPath": "System.Security.Cryptography.OpenSsl.dll",
        "name": "System.Security.Cryptography.OpenSsl.dll",
        "integrity": "sha256-aRvISk9WxFvJvGpMRvsVQlmgM+YmnWiDYUEi60fPOB0="
      },
      {
        "virtualPath": "System.Security.Cryptography.Primitives.dll",
        "name": "System.Security.Cryptography.Primitives.dll",
        "integrity": "sha256-w8HMV/or3iz8LFMdhiogdnAS8XzVxeRcoyKr4HkKN3M="
      },
      {
        "virtualPath": "System.Security.Cryptography.X509Certificates.dll",
        "name": "System.Security.Cryptography.X509Certificates.dll",
        "integrity": "sha256-Hy+c7p8kWbNl1/W9rSg2sKTTrIvwx4j1nGE2MuAK/A0="
      },
      {
        "virtualPath": "System.Security.Cryptography.dll",
        "name": "System.Security.Cryptography.dll",
        "integrity": "sha256-b6VOCASL8Gm7WK1yITjMUDTVrbVHmVbPJ7kiXYI00iY="
      },
      {
        "virtualPath": "System.Security.Principal.Windows.dll",
        "name": "System.Security.Principal.Windows.dll",
        "integrity": "sha256-ftGCDAlFnKIXLA4EICVBvXlQhyZfShkJneLXcaP1TBc="
      },
      {
        "virtualPath": "System.Security.Principal.dll",
        "name": "System.Security.Principal.dll",
        "integrity": "sha256-ryzeA33KkUbmAg4Pe29+Y28AUAjKwGZ1Bi6EU8reUs8="
      },
      {
        "virtualPath": "System.Security.SecureString.dll",
        "name": "System.Security.SecureString.dll",
        "integrity": "sha256-NkBDGoTX27fob96TRqpXws6rvLv4H1NrxzfrZbmAHug="
      },
      {
        "virtualPath": "System.Security.dll",
        "name": "System.Security.dll",
        "integrity": "sha256-m/+uCAWoOSC2X/h1AwmRZW5a1qdz7bppjQVVOdmktS8="
      },
      {
        "virtualPath": "System.ServiceModel.Web.dll",
        "name": "System.ServiceModel.Web.dll",
        "integrity": "sha256-9UOOrZI1GKcBCVUD4B1rirU+iyCqhbxH7NsRNvSjrqk="
      },
      {
        "virtualPath": "System.ServiceProcess.dll",
        "name": "System.ServiceProcess.dll",
        "integrity": "sha256-kc6vYAsrjfXTkhbtSj2XtcARyiG8Ne/1U2no8JEOBCc="
      },
      {
        "virtualPath": "System.Text.Encoding.CodePages.dll",
        "name": "System.Text.Encoding.CodePages.dll",
        "integrity": "sha256-e4wsbl8ZIMa5HwiGZbyTbua6a8u4x7wUAliz36b0awM="
      },
      {
        "virtualPath": "System.Text.Encoding.Extensions.dll",
        "name": "System.Text.Encoding.Extensions.dll",
        "integrity": "sha256-jmBCcRvhjuysDsZlbbIHvUVmY1E2CqmorMOT/j5gZbI="
      },
      {
        "virtualPath": "System.Text.Encoding.dll",
        "name": "System.Text.Encoding.dll",
        "integrity": "sha256-Gb7uvokfDzOE4Wm89gl5T5hn1owZ44amubVU3qUBKNk="
      },
      {
        "virtualPath": "System.Text.Encodings.Web.dll",
        "name": "System.Text.Encodings.Web.dll",
        "integrity": "sha256-3Sd+/4Ul2HUq35Zcp5bl4kMuXdh+4aO7xIJOisfZ7O4="
      },
      {
        "virtualPath": "System.Text.Json.dll",
        "name": "System.Text.Json.dll",
        "integrity": "sha256-yOfrGXMPd1yoS3mfn93Sn+KBInCcKNPbwfYod1yoty4="
      },
      {
        "virtualPath": "System.Text.RegularExpressions.dll",
        "name": "System.Text.RegularExpressions.dll",
        "integrity": "sha256-GFBb0yTZNATl1UbjWYes7T/8tYvQbol9tHV3jlTtSdY="
      },
      {
        "virtualPath": "System.Threading.AccessControl.dll",
        "name": "System.Threading.AccessControl.dll",
        "integrity": "sha256-I/valLUhwFj29i6aRKEuujz+3X9JZpMLZD4dB9xyxvs="
      },
      {
        "virtualPath": "System.Threading.Channels.dll",
        "name": "System.Threading.Channels.dll",
        "integrity": "sha256-gh5xm5Hjhw4dhN+eGodBP1ZqvXN4KK2T+bLBnEmxeUs="
      },
      {
        "virtualPath": "System.Threading.Overlapped.dll",
        "name": "System.Threading.Overlapped.dll",
        "integrity": "sha256-djS8P5grS2kr9kndHj06k0UXwDp0HubzcerckJaiYEQ="
      },
      {
        "virtualPath": "System.Threading.Tasks.Dataflow.dll",
        "name": "System.Threading.Tasks.Dataflow.dll",
        "integrity": "sha256-K63Aq4zjQs8VcLaNX1qD2P7kLIgcBOE3gHNKgZ7zTrM="
      },
      {
        "virtualPath": "System.Threading.Tasks.Extensions.dll",
        "name": "System.Threading.Tasks.Extensions.dll",
        "integrity": "sha256-IDV1gtfMW1lAH+ek3X8Ca7hcdsnNdFrCrit46xwdRVs="
      },
      {
        "virtualPath": "System.Threading.Tasks.Parallel.dll",
        "name": "System.Threading.Tasks.Parallel.dll",
        "integrity": "sha256-miOlG2UEkWajxnlxOOO853dP+qIcG/cfHxrThFanm28="
      },
      {
        "virtualPath": "System.Threading.Tasks.dll",
        "name": "System.Threading.Tasks.dll",
        "integrity": "sha256-npncFC+CDKHLFj9H/mV5WELYpIc+5MsgxCPG4jKpysU="
      },
      {
        "virtualPath": "System.Threading.Thread.dll",
        "name": "System.Threading.Thread.dll",
        "integrity": "sha256-N4X6UkLkquzwDmqPMZ2ys0xH00x/sebmdNLbSyZ09+I="
      },
      {
        "virtualPath": "System.Threading.ThreadPool.dll",
        "name": "System.Threading.ThreadPool.dll",
        "integrity": "sha256-FAl87J3hfGwaEG7Zt/QZXvlgdTyGt224RYqPDpn3mJI="
      },
      {
        "virtualPath": "System.Threading.Timer.dll",
        "name": "System.Threading.Timer.dll",
        "integrity": "sha256-33ElaN7K9ooaAV9KlYZOWKTr9ig8qFIe1Oi3OyZ6K+M="
      },
      {
        "virtualPath": "System.Threading.dll",
        "name": "System.Threading.dll",
        "integrity": "sha256-OKYxnlQTHUrA0ZHOKnHHSl0JaiCLmLqc5MJmzRdrkc0="
      },
      {
        "virtualPath": "System.Transactions.Local.dll",
        "name": "System.Transactions.Local.dll",
        "integrity": "sha256-clazZbfQnYNu8kTZJupq0KxbSDPyCaqJV4K7iYEUAn4="
      },
      {
        "virtualPath": "System.Transactions.dll",
        "name": "System.Transactions.dll",
        "integrity": "sha256-DD0S6pvlag3Ji4aeWrKucaWbshX+yF2+yPvkMdPbmus="
      },
      {
        "virtualPath": "System.ValueTuple.dll",
        "name": "System.ValueTuple.dll",
        "integrity": "sha256-wp1xc0tK5jjjiOXJZcZ/dia+d8DFXiT2XH1SBBIg2Qw="
      },
      {
        "virtualPath": "System.Web.HttpUtility.dll",
        "name": "System.Web.HttpUtility.dll",
        "integrity": "sha256-YvM0CXfSlotwTv1wJOw1/t66QahG6pEXyyTQl+tvrLA="
      },
      {
        "virtualPath": "System.Web.dll",
        "name": "System.Web.dll",
        "integrity": "sha256-ptvCV8pT505Ry5a2EWutIAE+HZq5EPseC0RH119k12k="
      },
      {
        "virtualPath": "System.Windows.dll",
        "name": "System.Windows.dll",
        "integrity": "sha256-E9AAMoFjM/dCglNGzagprwOOZdHL0gj5hoKqGb51tRk="
      },
      {
        "virtualPath": "System.Xml.Linq.dll",
        "name": "System.Xml.Linq.dll",
        "integrity": "sha256-Gj8NJedBKy4Yp3Cw8FdkphM4otXNnS69AXumlN7Ue8g="
      },
      {
        "virtualPath": "System.Xml.ReaderWriter.dll",
        "name": "System.Xml.ReaderWriter.dll",
        "integrity": "sha256-JJoufyRbsAyPtnUtnxxxM04n5mbTerZ5umrRAayx2F8="
      },
      {
        "virtualPath": "System.Xml.Serialization.dll",
        "name": "System.Xml.Serialization.dll",
        "integrity": "sha256-D8JZJ6CTOmHIjilqLipAvtkYDRd2+PV+oLAq/o5PVEY="
      },
      {
        "virtualPath": "System.Xml.XDocument.dll",
        "name": "System.Xml.XDocument.dll",
        "integrity": "sha256-K8Dp5eQiaS50n6PQUeHtjHJXbGMRwsQQ0ZeLWB+FRvY="
      },
      {
        "virtualPath": "System.Xml.XPath.XDocument.dll",
        "name": "System.Xml.XPath.XDocument.dll",
        "integrity": "sha256-xb/T1JaYi9vUJj9xv1LhiXp00njGk4+bcpCn9hEjA4A="
      },
      {
        "virtualPath": "System.Xml.XPath.dll",
        "name": "System.Xml.XPath.dll",
        "integrity": "sha256-tlw6CcyTApKZF9c1G9PMsuWBmUyEWAd89MGl1mnCDmI="
      },
      {
        "virtualPath": "System.Xml.XmlDocument.dll",
        "name": "System.Xml.XmlDocument.dll",
        "integrity": "sha256-ehYSNi88pVIkdduFTBnOUwC5fKILN+I0b5w36aBdgRw="
      },
      {
        "virtualPath": "System.Xml.XmlSerializer.dll",
        "name": "System.Xml.XmlSerializer.dll",
        "integrity": "sha256-wCsJqJLXIaQk6M2Ua/pR+k0r9tz0pp60W4pw6DVSGjs="
      },
      {
        "virtualPath": "System.Xml.dll",
        "name": "System.Xml.dll",
        "integrity": "sha256-9WTnsJtjzgrCxklXGQDEi97duMB25+zrJ1z0i4CBZfY="
      },
      {
        "virtualPath": "System.dll",
        "name": "System.dll",
        "integrity": "sha256-6rJGj+MlyMZ4v1IJNyqEmRlYyJn/YsV3CYwhXfCUER4="
      },
      {
        "virtualPath": "WindowsBase.dll",
        "name": "WindowsBase.dll",
        "integrity": "sha256-/z/8zM2U8cUqfUNjtVijroEdY4NoR6G2nimpENdfpgQ="
      },
      {
        "virtualPath": "Yaml2JsonNode.dll",
        "name": "Yaml2JsonNode.dll",
        "integrity": "sha256-vBXaStZhD++BLJBoO2bCZIOJTByVKPtAAHeH3bwYsM4="
      },
      {
        "virtualPath": "YamlDotNet.dll",
        "name": "YamlDotNet.dll",
        "integrity": "sha256-hvhz9oLGGaDCTiY9zdFf4CcQRmZkHJL1A1FmcdYe3aY="
      },
      {
        "virtualPath": "json-everything.net.dll",
        "name": "json-everything.net.dll",
        "integrity": "sha256-yYP7ka6yWlUi5KVHdd8PMZtgEbBbK95JjxRS3PIitgE="
      },
      {
        "virtualPath": "mscorlib.dll",
        "name": "mscorlib.dll",
        "integrity": "sha256-LK6Z9108PR0BmGiGNIQo7c3yygCRmJu2Lm+LA3PKFoo="
      },
      {
        "virtualPath": "netstandard.dll",
        "name": "netstandard.dll",
        "integrity": "sha256-jrOPHTUkdA26oJ5ScCDTkP5c0LvQ3A0ITRLdTvIKIp4="
      }
    ],
    "satelliteResources": {
      "cs": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-vHTbbkpM2kycWwUjHsT9LvK15O38jR5wci7JakU1nK0="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-loQUBqj//cfk+CBIk561GT0OU0VBGTaKToJmvd3+3qM="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-+2ynljycHBiAw0lH3jAYwbsCJSohQ78lUfqUYzrGpeU="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-hkC9Bad9Axw+EGuYo2jk9RYY4qrPECf54OKLJgmXg58="
        }
      ],
      "de": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-7/dNBd2wS5+YDjVmaxGtaCdyxkh94fvNglKzcfO3TEs="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-E1lhD7dHj7HdtLvQWeBBIML1xcWeHzhX4X2N0S1Wn8U="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-8u8SquSv90ntWWMHXahqqP097EOJDl/XsAvCJ9qtKLY="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-SqrlI30USrH/Qrxrj9mAZodiWOiA4aeG3mlUcj2FOGY="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-lNZ1f7OdY3QIyuAFaXeaWO1bqRz+F0hLfZw3teaG6oU="
        }
      ],
      "es": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-YT+2YujXNlahU97U5fAW1qWevIPj6uiBBHHofZoCp4Q="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-DgFaVKURSBWrQgIZsmwGD8ze7aNrYy+Z2chSerpE0gA="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-vk3H9dK2wHRgHUrPFAQ1D61AGW2O2wpOPQTVnqPdUw0="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-N8gc6yLprk1BMlHkP/gWkgg6FzpdcmLdv/r2ZQIY4TU="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-mZQNGHEFSuW72DHZP4J78/Rs+sE+xjfWML5EkM6A5Tc="
        }
      ],
      "fr": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-7iss2LdKMGEZ4Qjua4YW5boUeC4GMphsjzriMXsAH20="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-y1SoV559haF+CLtysaZ1Jk7pL1GwCh1emjuINn2oXvk="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-8yFzbw3AzJDCFq5pu6MUN8cWJmnFfFuJOiTUhqpxids="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-YDTv+4IWTEMDDwRSQAtTfYlVsemoS3rnbl96H3/s9yg="
        }
      ],
      "it": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-jW4NLUFrR25uXxPlX4vEFiygMPVjzLrPvst1m3ckkXs="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-u0Yd1Q3htc7bGtWPYB4yta/SZ0Kb6glx+rzeSl8gJZ4="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-aDyAi+nv1sh85qfeBtf8jfFSMk2z1yvc/84gmjgqda4="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-HlgCnPdBIHBbWU1BfGneFuDWVewr8DToglol5T8iiQ4="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-0e12/SP6w+A7py67ZrEp99wncc352LpEGy3UBG3Mt2k="
        }
      ],
      "ja": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-JTdzCtEP13q2ys/GPBtLF8k0Pm699wTSRhJyRCjW1cc="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-3dwbXkpUkmmeZlltU5jjunrv8MGkQXuu3lpE+EGfiWc="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-U+1jLTP6Tk8KtHebDUPlDo9hThzjSTC10es/EDxSfsA="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-Brr2iu7uju1qbW6M6QNu8+3P6xLeh2WD7duOrsYNsrs="
        }
      ],
      "ko": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-yPYKHw9uaaTaEtiZ1NINa7+wi0wk5AYqLXvq40BLAuk="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-KsX8Xe0lH2+HbxEhzpXOZ2Ps2bAsCSYmLBF1mnUUn0U="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-PW3vtc4H/RfLAMpWM+V6OmXlq3lncy1QavGhbgnwuqA="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-QHTV5IhzOUKCIkaI/dpXRdraDJmZuiA1zmdbK7F+nI8="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-AWb4DxpV2UfKQVHLcPNvDOrTn60fz2SOviQZw94flTg="
        }
      ],
      "nb-NO": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-k6o28Fqlkt8kdJ4UyI2ppdPjXDVnpZzXCc6+uOKauAU="
        }
      ],
      "pl-PL": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-nmYvANkpNoFNzFupCnZ0mWRm6NtxzBqntCLlVUAyjVk="
        }
      ],
      "pl": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-fA59UlJmEj3PbHtnMIqzY1A1mBHIpNi9+jb9oH3xpic="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-qTh+wxVxOn4yhhQlFZlUk55hObbe4XH+t4WyfR0xyoU="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-1RMcABEFvCvDpii5+puVvYNZxS/KLX/i3X5CfMHdqkM="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-+H0qSV2JGGhpbiTHJdLEgHeXV99+O9IrxmrpJyKPGBo="
        }
      ],
      "pt-BR": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-j4JVBBQwWoEphCJBs8NCJ/C46MvP2VBtk2blnQ30Mu8="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-p8HEMHrDS+ERY59a3foXKXJf53SHA/8krdVdS9o1hRs="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-kEmhU2fEYgUbTZsp10LbIbnXwW1NCYM+5aTGqgXXqs0="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-lfNz9wVawDQsKhFnEGiAFeo6ZizBm4qmbFSEQJqkk2k="
        }
      ],
      "pt": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-hgFM5gFiEsTZSK1+919i4HpJSyYqrJUh/I3ioJvbDk8="
        }
      ],
      "ru": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-qxMN77DC33ZzlfZVNHaaQmGCn8elBMixzmPeftu1qyk="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-mjgKmTMBtl9oZwwHS59A6QL9BbzQ34haZrwxJGKVwQ8="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-4YDzUNazihlGQEXCsfDD5AnTA4Q+AjIbi/z2HQYW1RM="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-RnQ89MYDOchgJ6DS7vNS0Fw1hyOyuyNISRR0rWsy9LY="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-NLd8I2JRYLNACYszBFmXE4/wQsqH/M+Q+b+rnesTL4o="
        }
      ],
      "sv-SE": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-EYevYrdBK0EaXlpuWYPTwvIHmv5dawZIQVoIaGqA2oE="
        }
      ],
      "tr-TR": [
        {
          "virtualPath": "JsonSchema.Net.resources.dll",
          "name": "JsonSchema.Net.resources.dll",
          "integrity": "sha256-tUl2dZoQrwLN5M+AbHfEmK+yWxV1ykS9YOwUlUM7QaI="
        }
      ],
      "tr": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-a2NJGUd0iY/S6tSik+mGb3KpzdeXi7d53Nw5aJbxgdM="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-wx75DzIFdVuse1McktLOeQ94lQo2D0OIViIlk+vdx4Q="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-zOHb6hHtdfnWJ4AoDdqQxhRdb/VdW53oclWU1bwIKQE="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-MssrAPZGCqo3KTCyWtx3Jwnp2T6pg6bKKV/1vBxrdHI="
        }
      ],
      "zh-Hans": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-Vc/R10GBWMIJKnmFdVW9FRN5c8UfAbEUtBSR4Qv/d24="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-uLPIo5FDQg3+yy9k5Ru1NhP1VPGEeiMkXo7dQCo8ybE="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-2WgVIP4wolnk8A5YCHr1p1xsXV6bmkYxEjrNFnDpGhc="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-JVWIPw/u/kSRdV43/rbLA+ddJEiogIEnC18kD1Zz6NA="
        }
      ],
      "zh-Hant": [
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.Scripting.resources.dll",
          "integrity": "sha256-/m+TeLM3w/YrGZPEWGp7ZiUqlsnCkuCVb10cMM1JXpo="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "name": "Microsoft.CodeAnalysis.CSharp.resources.dll",
          "integrity": "sha256-oblWPgsW2jr6G7gFFLs9kygiCLmsFNSvqaQpNpp9iaA="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "name": "Microsoft.CodeAnalysis.Scripting.resources.dll",
          "integrity": "sha256-W/C2TmDrfiEGajGsX/8BiDky+6aXTqGaORSpYQZNZhw="
        },
        {
          "virtualPath": "Microsoft.CodeAnalysis.resources.dll",
          "name": "Microsoft.CodeAnalysis.resources.dll",
          "integrity": "sha256-2PscfXE1Yg2dWsMwzJf6zokSzQr91Txy69eHXTU0tVk="
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
