const logicEditorName = 'editor-logic';
const dataEditorName = 'editor-data';
const outputEditorName = 'editor-output';

initializeEditor(logicEditorName);
initializeEditor(dataEditorName);
initializeEditor(outputEditorName);

const logicSample = {
	"if": [
		{
			"merge": [
				{ "missing": ["first_name", "last_name"] },
				{ "missing_some": [1, ["cell_phone", "home_phone"]] }
			]
		},
		"We require first name, last name, and one phone number.",
		"OK to proceed"
	]
}
const dataSample = { "first_name": "Bruce", "last_name": "Wayne" }

const logicEditor = ace.edit(logicEditorName);
var value = localStorage.getItem('logic.logic');
if (value) {
	logicEditor.setValue(value);
} else {
	logicEditor.setValue(JSON.stringify(logicSample, null, '\t'));
}
logicEditor.clearSelection();
logicEditor.getSession().on('change',
	() => {
		var text = logicEditor.getValue();
		if (text) {
			localStorage.setItem('logic.logic', text);
		} else {
			localStorage.removeItem('logic.logic');
		}
	});

const dataEditor = ace.edit(dataEditorName);
value = localStorage.getItem('logic.data');
if (value) {
	dataEditor.setValue(value);
} else {
	dataEditor.setValue(JSON.stringify(dataSample, null, '\t'));
}
dataEditor.clearSelection();
dataEditor.getSession().on('change',
	() => {
		var text = dataEditor.getValue();
		if (text) {
			localStorage.setItem('logic.data', text);
		} else {
			localStorage.removeItem('logic.data');
		}
	});

const outputEditor = ace.edit(outputEditorName);
outputEditor.setReadOnly(true);

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

	outputEditor.setValue('');

	const logic = getJsonFromEditor(logicEditor);
	const data = getJsonFromEditor(dataEditor);

	const response = await requestApplication(logic, data);

	if (response.errors) {
		outputElement.innerHTML = `<ol type="1" class="result-error text-left">${response.errors.map(getErrorElement).join('')}</ol>`;
	} else if (response.result !== undefined) {
		outputEditor.setValue(JSON.stringify(response.result, null, '\t'));
		outputEditor.clearSelection();
	}

	scrollToEnd();
}