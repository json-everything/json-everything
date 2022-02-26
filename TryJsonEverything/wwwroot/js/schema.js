const schemaEditorName = 'editor-schema';
const instanceEditorName = 'editor-instance';
const outputEditorName = 'editor-output';

initializeEditor(schemaEditorName);
initializeEditor(instanceEditorName);
initializeEditor(outputEditorName);

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

const outputEditor = ace.edit(outputEditorName);
outputEditor.setReadOnly(true);

async function requestValidation(schema, instance, options) {
	const body = {
		schema: schema,
		instance: instance,
		options: options
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

async function requestDataGeneration(schema) {
    const body = {
        schema: schema
	};

    const response = await fetch(`${baseUri}api/schema-data-generation`,
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

function transformDraft(selection) {
	return selection.replace('Draft ', '');
}

function transformOutputFormat(selection) {
	switch (selection) {
	case 'Flag (pass/fail)':
		return 'Flag';
	case 'Basic (list)':
		return 'Basic';
	case 'Detailed (condensed)':
		return 'Detailed';
	case 'Verbose (full)':
		return 'Verbose';
	default:
		return null;
	}
}

async function validate() {
	const outputElement = document.getElementById('output');
	outputElement.innerHTML = '';
	const outputEditorContainer = document.getElementById('editor-output');

	const draftElement = document.getElementById('validate-as');
	const outputFormatElement = document.getElementById('output-format');
	const baseUriElement = document.getElementById('base-uri');
	const requireFormatElement = document.getElementById('require-format');

	const schema = getJsonFromEditor(schemaEditor);
	const instance = getJsonFromEditor(instanceEditor);
	const options = {
		validateAs: transformDraft(draftElement.value),
		outputFormat: transformOutputFormat(outputFormatElement.value),
		defaultBaseUri: baseUriElement.value === '' ? null : baseUriElement.value,
		requireFormatValidation: requireFormatElement.checked
	};

	const response = await requestValidation(schema, instance, options);

	if (options.outputFormat === null) {
		outputEditorContainer.classList.add('collapse');
		if (response.result.valid) {
			outputElement.innerHTML = '<h3 class="result-valid">Instance is valid!</h3>';
		} else if (!response.result.errors) {
			outputElement.innerHTML = `<ol type="1" class="result-error text-left">${getErrorElement(response.result)}</ol>`;
		} else {
			outputElement.innerHTML = `<ol type="1" class="result-error text-left">${response.result.errors.map(getErrorElement).join('')}</ol>`;
		}
	} else {
		outputEditorContainer.classList.remove('collapse');
		outputEditor.setValue(JSON.stringify(response, null, '\t'));
		outputEditor.clearSelection();
	}

	scrollToEnd();
}

async function generateData() {
    const outputElement = document.getElementById('output');
    outputElement.innerHTML = '';

    const schema = getJsonFromEditor(schemaEditor);

	const response = await requestDataGeneration(schema);

	if (response.error) {
        outputElement.innerHTML = `<h3 class="result-valid">${response.error}</h3>`;
    } else {
        instanceEditor.setValue(JSON.stringify(response.result, null, '\t'));
        instanceEditor.clearSelection();
    }
}