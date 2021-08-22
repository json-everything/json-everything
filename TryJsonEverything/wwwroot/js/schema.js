const schemaEditorName = 'editor-schema';
const instanceEditorName = 'editor-instance';

function initializeEditor(name) {
	const editor = ace.edit(name);
	editor.setTheme('ace/theme/monokai');
	editor.session.setMode('ace/mode/json');
	editor.session.setOptions({
		theme: 'ace/theme/monokai',
		mode: 'ace/mode/json',
		tabSize: 2
	});
}

initializeEditor(schemaEditorName);
initializeEditor(instanceEditorName);

function getJsonFromEditor(editor) {
	const annotations = editor.getSession().getAnnotations();
	if (annotations.length !== 0) {
		return null;
	}
	const text = editor.getValue();
	return JSON.parse(text);
}

async function requestValidation(schema, instance) {
	const body = {
		schema: schema,
		instance: instance
	};

	const response = await fetch('https://localhost:5001/api/schema-validation',
		{
			method: 'POST',
			body: JSON.stringify(body),
			headers: {
				'Content-Type': 'application/json'
			}
		});
	return await response.json();
}

function getErrorElement(errorItem) {
	return `<li>
	<span class="font-weight-bold">${errorItem.error}</span>
	<div class="ml-4">Instance location: ${errorItem.instanceLocation}</div>
	<div class="ml-4">Schema location: ${errorItem.keywordLocation}</div>
</li>
`;
}

async function validate() {
	const outputElement = document.getElementById("output");
	outputElement.innerHTML = "";

	const schemaEditor = ace.edit(schemaEditorName);
	const instanceEditor = ace.edit(instanceEditorName);

	const schema = getJsonFromEditor(schemaEditor);
	const instance = getJsonFromEditor(instanceEditor);

	const response = await requestValidation(schema, instance);

	if (response.result.valid) {
		outputElement.innerHTML = '<h3 class="text-left schema-valid">Instance is valid!</h3>';
		return;
	}

	if (!response.result.errors) {
		outputElement.innerHTML = `<ol type="1" class="schema-error text-left">${getErrorElement(response.result)}</ol>`;
		return;
	}

	outputElement.innerHTML = `<ol type="1" class="schema-error text-left">${response.result.errors.map(getErrorElement)}</ol>`;
}