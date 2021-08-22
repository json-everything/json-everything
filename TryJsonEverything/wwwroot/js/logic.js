const logicEditorName = 'editor-logic';
const dataEditorName = 'editor-data';
const outputEditorName = 'editor-output';

initializeEditor(logicEditorName);
initializeEditor(dataEditorName);
initializeEditor(outputEditorName);

var schemaEditor = ace.edit(outputEditorName);
schemaEditor.setReadOnly(true);

async function requestApplication(logic, data) {
	const body = {
		logic: logic,
		data: data
	};

	const response = await fetch(`${baseUri}api/logic-apply`,
		{
			method: 'POST',
			body: JSON.stringify(body),
			headers: {
				'Content-Type': 'application/json'
			}
		});
	return await response.json();
}

function getErrorElement(error) {
	return `<li>
	<span class="font-weight-bold">${error}</span>
</li>
`;
}

async function apply() {
	const outputElement = document.getElementById("error-output");
	outputElement.innerHTML = '';

	const logicEditor = ace.edit(logicEditorName);
	const dataEditor = ace.edit(dataEditorName);
	const outputEditor = ace.edit(outputEditorName);
	outputEditor.setValue('');

	const logic = getJsonFromEditor(logicEditor);
	const data = getJsonFromEditor(dataEditor);

	const response = await requestApplication(logic, data);

	if (response.errors) {
		outputElement.innerHTML = `<ol type="1" class="result-error text-left">${response.errors.map(getErrorElement).join('')}</ol>`;
		return;
	}

	if (response.result) {
		outputEditor.setValue(JSON.stringify(response.result, null, '\t'));
		outputEditor.clearSelection();
	}
}