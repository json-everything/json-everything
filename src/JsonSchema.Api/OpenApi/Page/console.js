	/*
		The document is fetched from the route that serves it, so this file is a static
		asset with nothing substituted into it. The element carrying the page's
		configuration is written by the server.
	*/
	let doc = null;
	let operations = [];

	const config = JSON.parse(document.getElementById('oa-config').textContent);

	const state = {
		operation: null,
		language: 'curl',
		server: '',
		auth: {}
	};

	const inputs = new Map();

	function el(tag, className, text) {
		const node = document.createElement(tag);
		if (className) node.className = className;
		if (text != null) node.textContent = text;
		return node;
	}

	/* ---------- schema reading ---------- */

	function deref(schema) {
		if (!schema || !schema.$ref) return schema;

		const parts = schema.$ref.replace(/^#\//, '').split('/');
		let node = doc;
		for (const part of parts) {
			const key = decodeURIComponent(part).replace(/~1/g, '/').replace(/~0/g, '~');
			node = node && node[key];
		}
		return node || schema;
	}

	function schemaName(schema) {
		return schema && schema.$ref ? schema.$ref.split('/').pop() : null;
	}

	function typeLabel(schema) {
		if (!schema) return 'any';

		const resolved = deref(schema);
		if (Array.isArray(resolved.type)) return resolved.type.join(' | ');
		if (resolved.type === 'array') return 'array of ' + typeLabel(resolved.items);
		if (resolved.type) return resolved.type;
		if (resolved.const !== undefined) return typeof resolved.const;
		if (resolved.enum) return 'enum';
		return 'any';
	}

	function exampleFor(schema, depth) {
		depth = depth || 0;
		const resolved = deref(schema);
		if (!resolved || depth > 5) return null;

		if (resolved.const !== undefined) return resolved.const;
		if (resolved.enum) return resolved.enum[0];
		if (resolved.examples && resolved.examples.length) return resolved.examples[0];

		const type = Array.isArray(resolved.type) ? resolved.type[0] : resolved.type;

		if (type === 'object' || resolved.properties) {
			const value = {};
			for (const [key, child] of Object.entries(resolved.properties || {})) {
				value[key] = exampleFor(child, depth + 1);
			}
			return value;
		}

		if (type === 'array') return [exampleFor(resolved.items, depth + 1)].filter(x => x !== null);
		if (type === 'integer' || type === 'number') return 0;
		if (type === 'boolean') return true;
		if (type === 'null') return null;

		if (resolved.format === 'date-time') return new Date().toISOString();
		if (resolved.format === 'uuid') return '00000000-0000-0000-0000-000000000000';

		return 'string';
	}

	/* ---------- schema rendering ---------- */

	function renderSchema(schema, mediaType) {
		const resolved = deref(schema);
		const box = el('div', 'oa-schema');

		const header = el('div', 'oa-schema-header');
		header.append(el('span', null, schemaName(schema) || typeLabel(resolved)));
		if (mediaType) header.append(el('span', 'oa-schema-media', mediaType));
		box.append(header);

		const properties = resolved.properties || {};
		const required = new Set(resolved.required || []);

		if (!Object.keys(properties).length) {
			const row = el('div', 'oa-property');
			row.append(el('div', 'oa-property-text', 'No documented properties.'));
			box.append(row);
		}

		for (const [name, child] of Object.entries(properties)) {
			box.append(renderProperty(name, child, required.has(name)));
		}

		if (resolved.additionalProperties === false) {
			box.append(el('div', 'oa-schema-footer', 'No additional properties permitted.'));
		}

		return box;
	}

	function renderProperty(name, schema, isRequired) {
		const resolved = deref(schema);
		const row = el('div', 'oa-property');

		const line = el('div', 'oa-property-line');
		line.append(el('span', 'oa-property-name', name));
		line.append(el('span', 'oa-property-type', typeLabel(schema)));
		if (isRequired) line.append(el('span', 'oa-badge oa-badge-required', 'required'));
		if (resolved && resolved.const !== undefined) {
			line.append(el('span', 'oa-badge oa-badge-const', '= ' + resolved.const));
		}
		row.append(line);

		if (resolved && resolved.description) {
			row.append(el('div', 'oa-property-text', resolved.description));
		}

		const nested = resolved && resolved.properties;
		if (nested && Object.keys(nested).length) {
			const children = el('div', 'oa-property-nested');
			const required = new Set(resolved.required || []);
			for (const [key, child] of Object.entries(nested)) {
				children.append(renderProperty(key, child, required.has(key)));
			}
			row.append(children);
		}

		return row;
	}

	/* ---------- operations ---------- */

	const METHODS = ['get', 'post', 'put', 'patch', 'delete', 'head', 'options', 'trace'];

	function collectOperations() {
		const found = [];
		for (const [path, item] of Object.entries(doc.paths || {})) {
			for (const method of METHODS) {
				if (!item[method]) continue;
				found.push({
					path,
					method,
					definition: item[method],
					id: (method + '-' + path).replace(/[^a-z0-9]+/gi, '-').toLowerCase()
				});
			}
		}
		return found;
	}

	function bodySchema(operation) {
		const content = operation.definition.requestBody && operation.definition.requestBody.content;
		if (!content) return null;
		if (content['application/json']) return content['application/json'].schema;
		const first = Object.values(content)[0];
		return first ? first.schema : null;
	}

	function parameters(operation) {
		return (operation.definition.parameters || []).map(deref);
	}

	function statusClass(code) {
		const first = String(code)[0];
		if (first === '5') return 'oa-status oa-status-server';
		if (first === '4') return 'oa-status oa-status-client';
		return 'oa-status oa-status-success';
	}

	/* ---------- navigation ---------- */

	function buildNavigation() {
		const nav = document.getElementById('oa-nav');
		nav.textContent = '';

		const list = el('div', 'oa-nav-group');

		for (const operation of operations) {
			const link = el('button', 'oa-nav-link');
			link.type = 'button';
			link.dataset.operation = operation.id;

			const method = el('span', 'oa-nav-method', operation.method.toUpperCase());
			method.style.color = 'var(--oa-' + operation.method + ')';
			link.append(method);

			link.append(el('span', 'oa-nav-label', operation.path));
			link.addEventListener('click', () => selectOperation(operation.id, true));
			list.append(link);
		}

		nav.append(list);
	}

	function markCurrent() {
		for (const link of document.querySelectorAll('.oa-nav-link')) {
			link.setAttribute('aria-current', String(link.dataset.operation === state.operation));
		}
	}

	/* ---------- documentation ---------- */

	function buildDocumentation() {
		const docs = document.getElementById('oa-docs');
		docs.textContent = '';

		const intro = el('div', 'oa-intro');
		intro.append(el('h1', 'oa-intro-title', doc.info.title));
		if (doc.info.description) intro.append(el('p', 'oa-intro-text', doc.info.description));

		const callout = el('div', 'oa-callout');
		callout.append(document.createTextNode('Bodies are described with complete JSON Schema. Constraints such as '));
		callout.append(el('code', null, 'const'));
		callout.append(document.createTextNode(', '));
		callout.append(el('code', null, 'required'));
		callout.append(document.createTextNode(' and '));
		callout.append(el('code', null, 'additionalProperties'));
		callout.append(document.createTextNode(' are enforced by the server, not only documented here.'));
		intro.append(callout);

		docs.append(intro);

		for (const operation of operations) {
			docs.append(renderOperation(operation));
		}
	}

	function renderOperation(operation) {
		const section = el('section', 'oa-operation');
		section.id = operation.id;
		section.dataset.operation = operation.id;

		const header = el('div', 'oa-operation-header');
		header.append(el('span', 'oa-method oa-method-' + operation.method, operation.method.toUpperCase()));

		const path = el('span', 'oa-path');
		for (const piece of operation.path.split(/(\{[^}]+\})/)) {
			if (!piece) continue;
			path.append(piece.startsWith('{')
				? el('span', 'oa-path-parameter', piece)
				: document.createTextNode(piece));
		}
		header.append(path);
		section.append(header);

		const meta = el('div', 'oa-operation-meta');

		if (operation.definition.operationId) {
			meta.append(el('span', 'oa-operation-id', operation.definition.operationId));
		}

		for (const tag of operation.definition.tags || []) {
			meta.append(el('span', 'oa-tag', tag));
		}

		if (meta.childElementCount) section.append(meta);
		if (operation.definition.summary) {
			section.append(el('p', 'oa-operation-text', operation.definition.summary));
		}
		if (operation.definition.description) {
			section.append(el('p', 'oa-operation-text', operation.definition.description));
		}

		const params = parameters(operation);
		if (params.length) {
			const block = el('div', 'oa-section');
			block.append(el('div', 'oa-section-heading', 'Parameters'));

			const box = el('div', 'oa-schema');
			for (const parameter of params) {
				const row = el('div', 'oa-property');
				const line = el('div', 'oa-property-line');
				line.append(el('span', 'oa-property-name', parameter.name));
				line.append(el('span', 'oa-property-type', parameter.in + ' · ' + typeLabel(parameter.schema)));
				if (parameter.required) line.append(el('span', 'oa-badge oa-badge-required', 'required'));
				row.append(line);
				if (parameter.description) row.append(el('div', 'oa-property-text', parameter.description));
				box.append(row);
			}

			block.append(box);
			section.append(block);
		}

		const body = bodySchema(operation);
		if (body) {
			const block = el('div', 'oa-section');
			block.append(el('div', 'oa-section-heading', 'Request body'));
			block.append(renderSchema(body, 'application/json'));
			section.append(block);
		}

		const responses = operation.definition.responses || {};
		if (Object.keys(responses).length) {
			const block = el('div', 'oa-section');
			block.append(el('div', 'oa-section-heading', 'Responses'));

			for (const [code, raw] of Object.entries(responses)) {
				const response = deref(raw);
				const card = el('div', 'oa-response');

				const header = el('div', 'oa-response-header');
				header.append(el('span', statusClass(code), code));
				header.append(el('span', 'oa-response-text', response.description || ''));
				card.append(header);

				const content = response.content || {};
				const mediaType = Object.keys(content)[0];
				if (mediaType && content[mediaType].schema) {
					const body = el('div', 'oa-response-body');
					body.append(renderSchema(content[mediaType].schema, mediaType));
					card.append(body);
				}

				block.append(card);
			}

			section.append(block);
		}

		return section;
	}

	/* ---------- request shape ---------- */

	function securitySchemes() {
		return (doc.components && doc.components.securitySchemes) || {};
	}

	function authHeader() {
		for (const [name, raw] of Object.entries(securitySchemes())) {
			const scheme = deref(raw);
			const value = state.auth[name];
			if (!value) continue;

			if (scheme.type === 'http' && scheme.scheme === 'bearer') return ['Authorization', 'Bearer ' + value];
			if (scheme.type === 'http' && scheme.scheme === 'basic') return ['Authorization', 'Basic ' + value];
			if (scheme.type === 'apiKey' && scheme.in === 'header') return [scheme.name, value];
		}
		return null;
	}

	function authQuery() {
		for (const [name, raw] of Object.entries(securitySchemes())) {
			const scheme = deref(raw);
			const value = state.auth[name];
			if (value && scheme.type === 'apiKey' && scheme.in === 'query') return [scheme.name, value];
		}
		return null;
	}

	function parameterValue(operation, name) {
		const node = inputs.get(operation.id + ':parameter:' + name);
		return node ? node.value.trim() : '';
	}

	function bodyValue(operation) {
		const node = inputs.get(operation.id + ':body');
		if (node && node.value.trim()) return node.value;

		const schema = bodySchema(operation);
		return schema ? JSON.stringify(exampleFor(schema), null, 2) : null;
	}

	function requestShape(operation) {
		let path = operation.path;
		const query = [];
		const headers = [];

		for (const parameter of parameters(operation)) {
			const value = parameterValue(operation, parameter.name);
			if (parameter.in === 'path') {
				path = path.replace('{' + parameter.name + '}', encodeURIComponent(value || '{' + parameter.name + '}'));
			} else if (parameter.in === 'query' && value) {
				query.push([parameter.name, value]);
			} else if (parameter.in === 'header' && value) {
				headers.push([parameter.name, value]);
			}
		}

		const queryAuth = authQuery();
		if (queryAuth) query.push(queryAuth);

		const search = query.length
			? '?' + query.map(([k, v]) => k + '=' + encodeURIComponent(v)).join('&')
			: '';

		const body = bodySchema(operation) ? bodyValue(operation) : null;
		if (body) headers.unshift(['Content-Type', 'application/json']);

		const headerAuth = authHeader();
		if (headerAuth) headers.push(headerAuth);

		return {
			url: state.server + path + search,
			method: operation.method.toUpperCase(),
			headers,
			body
		};
	}

	/* ---------- code samples ---------- */

	const LANGUAGES = [
		['curl', 'cURL'],
		['javascript', 'JavaScript'],
		['csharp', 'C#'],
		['python', 'Python'],
		['go', 'Go']
	];

	function codeSample(operation, language) {
		const request = requestShape(operation);

		if (language === 'curl') return curlSample(request);
		if (language === 'javascript') return javascriptSample(request);
		if (language === 'csharp') return csharpSample(request);
		if (language === 'python') return pythonSample(request);
		if (language === 'go') return goSample(request);
		return '';
	}

	function curlSample(request) {
		const lines = ["curl -X " + request.method + " '" + request.url + "'"];
		for (const [name, value] of request.headers) lines.push("  -H '" + name + ": " + value + "'");
		if (request.body) lines.push("  -d '" + compact(request.body) + "'");
		return lines.join(' \\\n');
	}

	function javascriptSample(request) {
		const parts = ["const response = await fetch('" + request.url + "', {", "  method: '" + request.method + "'"];

		if (request.headers.length) {
			const headers = request.headers.map(([name, value]) => "    '" + name + "': '" + value + "'").join(',\n');
			parts.push('  headers: {\n' + headers + '\n  }');
		}

		if (request.body) parts.push('  body: JSON.stringify(' + indent(request.body, '  ') + ')');

		return parts.join(',\n') + '\n});\n\nconst data = await response.json();';
	}

	function csharpSample(request) {
		const lines = ['using var client = new HttpClient();'];

		for (const [name, value] of request.headers) {
			if (name === 'Content-Type') continue;
			lines.push('client.DefaultRequestHeaders.Add("' + name + '", "' + value + '");');
		}

		lines.push('');

		if (request.body) {
			lines.push('var payload = JsonNode.Parse("""');
			lines.push(request.body);
			lines.push('""");');
			lines.push('');
			lines.push('var response = await client.' + csharpMethod(request.method) + 'AsJsonAsync("' + request.url + '", payload);');
		} else {
			lines.push('var response = await client.' + csharpMethod(request.method) + 'Async("' + request.url + '");');
		}

		lines.push('response.EnsureSuccessStatusCode();');
		return lines.join('\n');
	}

	function csharpMethod(method) {
		if (method === 'POST') return 'Post';
		if (method === 'PUT') return 'Put';
		if (method === 'DELETE') return 'Delete';
		if (method === 'PATCH') return 'Patch';
		return 'Get';
	}

	function pythonSample(request) {
		const lines = ['import requests', ''];

		if (request.headers.length) {
			lines.push('headers = {');
			for (const [name, value] of request.headers) lines.push('    "' + name + '": "' + value + '",');
			lines.push('}');
			lines.push('');
		}

		if (request.body) {
			lines.push('payload = ' + request.body.replace(/\btrue\b/g, 'True').replace(/\bfalse\b/g, 'False').replace(/\bnull\b/g, 'None'));
			lines.push('');
		}

		const args = ['"' + request.url + '"'];
		if (request.headers.length) args.push('headers=headers');
		if (request.body) args.push('json=payload');

		lines.push('response = requests.' + request.method.toLowerCase() + '(' + args.join(', ') + ')');
		lines.push('response.raise_for_status()');
		return lines.join('\n');
	}

	function goSample(request) {
		const lines = [];

		if (request.body) {
			lines.push('payload := []byte(`' + compact(request.body) + '`)');
			lines.push('req, _ := http.NewRequest("' + request.method + '", "' + request.url + '", bytes.NewBuffer(payload))');
		} else {
			lines.push('req, _ := http.NewRequest("' + request.method + '", "' + request.url + '", nil)');
		}

		for (const [name, value] of request.headers) lines.push('req.Header.Set("' + name + '", "' + value + '")');

		lines.push('');
		lines.push('resp, err := http.DefaultClient.Do(req)');
		lines.push('if err != nil {');
		lines.push('\treturn err');
		lines.push('}');
		lines.push('defer resp.Body.Close()');
		return lines.join('\n');
	}

	function compact(json) {
		try {
			return JSON.stringify(JSON.parse(json));
		} catch {
			return json.replace(/\s+/g, ' ');
		}
	}

	function indent(text, prefix) {
		return text.split('\n').join('\n' + prefix);
	}

	/* ---------- rail ---------- */

	function buildRail() {
		const rail = document.getElementById('oa-rail');
		rail.textContent = '';

		const operation = operations.find(o => o.id === state.operation);
		if (!operation) return;

		rail.append(buildCodePanel(operation));
		rail.append(buildConsolePanel(operation));

		const auth = buildAuthPanel();
		if (auth) rail.append(auth);
	}

	function buildCodePanel(operation) {
		const panel = el('div', 'oa-panel');

		const header = el('div', 'oa-panel-header');
		header.append(el('h3', 'oa-panel-title', 'Request'));

		const copy = el('button', 'oa-copy', 'Copy');
		copy.type = 'button';
		header.append(copy);
		panel.append(header);

		const languages = el('div', 'oa-languages');
		for (const [id, label] of LANGUAGES) {
			const button = el('button', 'oa-language', label);
			button.type = 'button';
			button.setAttribute('aria-pressed', String(state.language === id));
			button.addEventListener('click', () => {
				state.language = id;
				buildRail();
			});
			languages.append(button);
		}
		panel.append(languages);

		const code = el('pre', 'oa-code');
		code.textContent = codeSample(operation, state.language);
		panel.append(code);

		copy.addEventListener('click', async () => {
			try {
				await navigator.clipboard.writeText(code.textContent);
				copy.textContent = 'Copied';
			} catch {
				const range = document.createRange();
				range.selectNodeContents(code);
				const selection = window.getSelection();
				selection.removeAllRanges();
				selection.addRange(range);
				copy.textContent = 'Selected';
			}
			setTimeout(() => { copy.textContent = 'Copy'; }, 1400);
		});

		return panel;
	}

	function buildConsolePanel(operation) {
		const panel = el('div', 'oa-panel');

		const header = el('div', 'oa-panel-header');
		header.append(el('h3', 'oa-panel-title', 'Send a request'));
		panel.append(header);

		const body = el('div', 'oa-panel-body');

		for (const parameter of parameters(operation)) {
			const field = el('div', 'oa-field');

			const label = el('label', 'oa-field-label');
			label.htmlFor = operation.id + '-' + parameter.name;
			label.append(document.createTextNode(parameter.name + ' '));
			label.append(el('span', 'oa-property-type', parameter.in));
			if (parameter.required) label.append(el('span', 'oa-field-required', ' *'));
			field.append(label);

			const input = el('input', 'oa-input');
			input.type = 'text';
			input.id = operation.id + '-' + parameter.name;
			input.placeholder = typeLabel(parameter.schema);
			input.addEventListener('input', () => refreshCode(operation));
			inputs.set(operation.id + ':parameter:' + parameter.name, input);
			field.append(input);

			body.append(field);
		}

		const schema = bodySchema(operation);
		if (schema) {
			const field = el('div', 'oa-field');

			const label = el('label', 'oa-field-label', 'body · application/json');
			label.htmlFor = operation.id + '-body';
			field.append(label);

			const textarea = el('textarea', 'oa-textarea');
			textarea.id = operation.id + '-body';
			textarea.value = JSON.stringify(exampleFor(schema), null, 2);
			textarea.addEventListener('input', () => refreshCode(operation));
			inputs.set(operation.id + ':body', textarea);
			field.append(textarea);

			body.append(field);
		}

		const send = el('button', 'oa-send', 'Send ' + operation.method.toUpperCase());
		send.type = 'button';
		body.append(send);

		const result = el('div');
		result.hidden = true;
		body.append(result);

		send.addEventListener('click', () => sendRequest(operation, send, result));

		panel.append(body);
		return panel;
	}

	function buildAuthPanel() {
		const schemes = securitySchemes();
		if (!Object.keys(schemes).length) return null;

		const panel = el('div', 'oa-panel');
		panel.id = 'oa-auth-panel';

		const header = el('div', 'oa-panel-header');
		header.append(el('h3', 'oa-panel-title', 'Authentication'));
		panel.append(header);

		const body = el('div', 'oa-panel-body');

		for (const [name, raw] of Object.entries(schemes)) {
			const scheme = deref(raw);
			const field = el('div', 'oa-field');

			const label = el('label', 'oa-field-label');
			label.htmlFor = 'oa-auth-' + name;
			label.append(document.createTextNode(name + ' '));
			label.append(el('span', 'oa-property-type', describeScheme(scheme)));
			field.append(label);

			const input = el('input', 'oa-input');
			input.type = 'password';
			input.id = 'oa-auth-' + name;
			input.placeholder = scheme.type === 'apiKey' ? 'key' : 'token';
			input.value = state.auth[name] || '';
			input.addEventListener('input', () => {
				state.auth[name] = input.value.trim();
				updateAuthState();
				const operation = operations.find(o => o.id === state.operation);
				if (operation) refreshCode(operation);
			});
			field.append(input);

			if (scheme.description) field.append(el('div', 'oa-hint', scheme.description));

			body.append(field);
		}

		body.append(el('div', 'oa-hint', 'Credentials stay in this page. They are never stored, and go only to the request you send.'));

		panel.append(body);
		return panel;
	}

	function describeScheme(scheme) {
		if (scheme.type === 'http') return scheme.scheme + (scheme.bearerFormat ? ' · ' + scheme.bearerFormat : '');
		if (scheme.type === 'apiKey') return scheme.in + ' · ' + scheme.name;
		return scheme.type;
	}

	function refreshCode(operation) {
		const code = document.querySelector('#oa-rail .oa-code');
		if (code) code.textContent = codeSample(operation, state.language);
	}

	function updateAuthState() {
		const isSet = Object.values(state.auth).some(value => value);
		const badge = document.getElementById('oa-auth-state');
		badge.textContent = isSet ? 'set' : 'off';
		badge.dataset.set = String(isSet);
	}

	/* ---------- sending ---------- */

	async function sendRequest(operation, button, result) {
		const request = requestShape(operation);

		button.disabled = true;
		button.textContent = 'Sending…';

		result.hidden = false;
		result.className = 'oa-result';
		result.textContent = '';

		const started = performance.now();

		try {
			const response = await fetch(request.url, {
				method: request.method,
				headers: Object.fromEntries(request.headers),
				body: request.body && request.method !== 'GET' && request.method !== 'HEAD' ? request.body : undefined
			});

			const elapsed = Math.round(performance.now() - started);
			const text = await response.text();

			const line = el('div', 'oa-result-line');
			line.append(el('span', statusClass(response.status), String(response.status)));
			if (response.statusText) line.append(el('span', 'oa-response-text', response.statusText));
			line.append(el('span', 'oa-result-duration', elapsed + ' ms'));
			result.append(line);

			const code = el('pre', 'oa-code');
			try {
				code.textContent = JSON.stringify(JSON.parse(text), null, 2);
			} catch {
				code.textContent = text || '(empty response)';
			}
			result.append(code);
		} catch (error) {
			const line = el('div', 'oa-result-line');
			line.append(el('span', 'oa-status oa-status-server', 'failed'));
			result.append(line);
			result.append(el('div', 'oa-hint', error.message + '. A browser blocks cross-origin requests unless the server sends CORS headers; the generated code above has no such restriction.'));
		} finally {
			button.disabled = false;
			button.textContent = 'Send ' + operation.method.toUpperCase();
		}
	}

	/* ---------- selection ---------- */

	function selectOperation(id, scroll) {
		state.operation = id;
		markCurrent();
		buildRail();

		if (scroll) {
			const target = document.getElementById(id);
			if (target) target.scrollIntoView({ behavior: 'smooth', block: 'start' });
		}
	}

	function buildServers() {
		const select = document.getElementById('oa-server');
		const servers = doc.servers && doc.servers.length
			? doc.servers
			: [{ url: '', description: 'Same origin' }];

		for (const server of servers) {
			const option = el('option', null, server.description ? server.description + ' — ' + server.url : server.url);
			option.value = server.url;
			select.append(option);
		}

		state.server = servers[0].url;

		select.addEventListener('change', () => {
			state.server = select.value;
			const operation = operations.find(o => o.id === state.operation);
			if (operation) refreshCode(operation);
		});
	}

	function watchScroll() {
		const observer = new IntersectionObserver(entries => {
			for (const entry of entries) {
				if (!entry.isIntersecting) continue;
				const id = entry.target.dataset.operation;
				if (id && id !== state.operation) selectOperation(id, false);
				break;
			}
		}, { rootMargin: '-70px 0px -65% 0px', threshold: 0 });

		for (const section of document.querySelectorAll('.oa-operation')) observer.observe(section);
	}

	/* ---------- theme ---------- */

	const THEME_KEY = 'oa-theme';

	function storedTheme() {
		try {
			return localStorage.getItem(THEME_KEY);
		} catch {
			return null;
		}
	}

	function storeTheme(value) {
		try {
			localStorage.setItem(THEME_KEY, value);
		} catch {
			// Storage can be unavailable or full; the choice then lasts for this page only.
		}
	}

	function applyStoredTheme() {
		const stored = storedTheme();
		if (stored === 'dark' || stored === 'light') {
			document.documentElement.setAttribute('data-theme', stored);
		}
	}

	function toggleTheme() {
		const root = document.documentElement;
		const isDark = root.getAttribute('data-theme') === 'dark' ||
			(!root.hasAttribute('data-theme') && window.matchMedia('(prefers-color-scheme: dark)').matches);

		const next = isDark ? 'light' : 'dark';
		root.setAttribute('data-theme', next);
		storeTheme(next);
	}

	/* ---------- start ---------- */

	function reportFailure(message) {
		const docs = document.getElementById('oa-docs');
		docs.textContent = '';

		const intro = el('div', 'oa-intro');
		intro.append(el('h1', 'oa-intro-title', 'The description could not be loaded'));
		intro.append(el('p', 'oa-intro-text', message));
		docs.append(intro);
	}

	async function start() {
		applyStoredTheme();

		document.getElementById('oa-theme').addEventListener('click', toggleTheme);

		document.getElementById('oa-auth-jump').addEventListener('click', () => {
			const panel = document.getElementById('oa-auth-panel');
			if (panel) panel.scrollIntoView({ behavior: 'smooth', block: 'center' });
		});

		let response;
		try {
			response = await fetch(config.documentUrl, { headers: { Accept: 'application/json' } });
		} catch (error) {
			reportFailure('Could not reach ' + config.documentUrl + '. ' + error.message);
			return;
		}

		if (!response.ok) {
			reportFailure('Requesting ' + config.documentUrl + ' returned ' + response.status + '.');
			return;
		}

		try {
			doc = await response.json();
		} catch (error) {
			reportFailure(config.documentUrl + ' did not return valid JSON. ' + error.message);
			return;
		}

		operations = collectOperations();

		document.getElementById('oa-doc-title').textContent = doc.info.title;
		document.getElementById('oa-doc-version').textContent = 'v' + doc.info.version;

		buildServers();
		buildNavigation();
		buildDocumentation();

		state.operation = operations.length ? operations[0].id : null;
		markCurrent();
		buildRail();
		updateAuthState();
		watchScroll();
	}

	start();
