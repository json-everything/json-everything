const schemaEditorName = 'editor-schema';
const instanceEditorName = 'editor-instance';

initializeEditor(schemaEditorName);
initializeEditor(instanceEditorName);

const schemaSample = {
	"$id": "https://example.com/person.schema.json",
	"$schema": "https://json-schema.org/draft/2020-12/schema",
	"title": "Person",
	"type": "object",
	"properties": {
		"firstName": {
			"type": "string",
			"description": "The person's first name."
		},
		"lastName": {
			"type": "string",
			"description": "The person's last name."
		},
		"age": {
			"description": "Age in years which must be equal to or greater than zero.",
			"type": "integer",
			"minimum": 0
		}
	}
};
const instanceSample = {
	"firstName": "John",
	"lastName": "Doe",
	"age": 21
};

const schemaEditor = ace.edit(schemaEditorName);
var value = localStorage.getItem('schema.schema');
if (value) {
	schemaEditor.setValue(value);
} else {
	schemaEditor.setValue(JSON.stringify(schemaSample, null, '\t'));
}
schemaEditor.clearSelection();
schemaEditor.getSession().on('change', () => localStorage.setItem('schema.schema', schemaEditor.getValue()));

const instanceEditor = ace.edit(instanceEditorName);
value = localStorage.getItem('schema.instance');
if (value) {
	instanceEditor.setValue(value);
} else {
	instanceEditor.setValue(JSON.stringify(instanceSample, null, '\t'));
}
instanceEditor.clearSelection();
instanceEditor.getSession().on('change', () => localStorage.setItem('schema.instance', instanceEditor.getValue()));

async function requestValidation(schema, instance) {
	const body = {
		schema: schema,
		instance: instance
	};

	const response = await fetch(`${baseUri}api/schema-validation`,
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

	const schema = getJsonFromEditor(schemaEditor);
	const instance = getJsonFromEditor(instanceEditor);

	const response = await requestValidation(schema, instance);

	if (response.result.valid) {
		outputElement.innerHTML = '<h3 class="result-valid">Instance is valid!</h3>';
	} else if (!response.result.errors) {
		outputElement.innerHTML = `<ol type="1" class="result-error text-left">${getErrorElement(response.result)}</ol>`;
	} else {
		outputElement.innerHTML = `<ol type="1" class="result-error text-left">${response.result.errors.map(getErrorElement).join('')}</ol>`;
	}

	scrollToEnd();
}