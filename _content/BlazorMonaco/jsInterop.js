var require = { paths: { vs: document.baseURI + '_content/BlazorMonaco/lib/monaco-editor/min/vs' } };
window.blazorMonaco = window.blazorMonaco || {};
window.blazorMonaco.editors = [];

window.blazorMonaco.editor = {

    //#region Utilities

    getEditorHolder: function (id, silent = false) {
        let editorHolder = window.blazorMonaco.editors.find(h => h.id === id);

        let error = editorHolder == null
            ? "Couldn't find the editor with id: " + id + " editors.length: " + window.blazorMonaco.editors.length
            : editorHolder.editor == null
                ? "editor is null for editorHolder: " + editorHolder
                : null;
        if (error != null) {
            if (silent)
                return null;
            throw error;
        }

        return editorHolder;
    },

    getEditor: function (id, silent = false) {
        let editorHolder = this.getEditorHolder(id, silent);
        return editorHolder == null ? null : editorHolder.editor;
    },

    getEditorLineNumber: function (editorId, lineNumber) {
        const editorHolder = this.getEditorHolder(editorId, true);
        if (editorHolder == null) {
            return "";
        }

        let lineNumbersCache = editorHolder.lineNumbersCache;
        if (lineNumbersCache == null) {
            lineNumbersCache = editorHolder.lineNumbersCache = {};
            lineNumbersCache.Promises = {};
        }

        let lineNumberVal = lineNumbersCache[lineNumber];
        if (lineNumberVal != null) {
            return lineNumberVal;
        }

        lineNumbersCache[lineNumber] = "";
        lineNumbersCache.Promises[lineNumber] = editorHolder.dotnetRef.invokeMethodAsync("LineNumbersCallback", lineNumber)
            .then(result => {
                lineNumbersCache[lineNumber] = result;
                delete lineNumbersCache.Promises[lineNumber];
                if (Object.keys(lineNumbersCache.Promises).length == 0) {
                    this.redrawLineNumbers(editorId);
                }
            })
            .catch(err => {
                console.log("BlazorMonaco: LineNumbersCallback(" + lineNumber + ") failed with error : " + err);
                delete lineNumbersCache.Promises[lineNumber];
                if (Object.keys(lineNumbersCache.Promises).length == 0) {
                    this.redrawLineNumbers(editorId);
                }
            });
        return "";
    },

    reloadLineNumbers: function (editorId) {
        const editorHolder = this.getEditorHolder(editorId, true);
        if (editorHolder == null) {
            return;
        }
        editorHolder.lineNumbersCache = null;
        this.redrawLineNumbers(editorId);
    },

    redrawLineNumbers: function (editorId) {
        const editor = this.getEditor(editorId, true);
        if (editor == null) {
            return;
        }
        const optionValue = editor.getOption(monaco.editor.EditorOptions.lineNumbers.id);
        if (optionValue.renderType === monaco.editor.RenderLineNumbersType.Custom) {
            editor.updateOptions({
                lineNumbers: (lineNumber) => {
                    return this.getEditorLineNumber(editorId, lineNumber);
                }
            });
        }
    },

    convertUriToString: function (obj) {
        if (obj == null)
            return obj;
        if (Array.isArray(obj))
            return obj.map(this.convertUriToString);

        Object.keys(obj).forEach(k => {
            if (monaco.Uri.isUri(obj[k]))
                obj[k] = obj[k].toString();
            else if (typeof obj[k] === 'object')
                blazorMonaco.editor.convertUriToString(obj[k]);
        });

        return obj;
    },

    removeCircularReferences: function (orig) {
        if (Array.isArray(orig))
            return orig.map(this.removeCircularReferences);

        const circularReferenceReplacer = () => {
            const seen = new WeakSet();
            return (key, value) => {
                if (typeof value === "object" && value !== null) {
                    if (seen.has(value)) {
                        return;
                    }
                    seen.add(value);
                }
                return value;
            };
        };

        let json_str = JSON.stringify(orig, circularReferenceReplacer());
        return JSON.parse(json_str);
    },

    setWasm: function (isWasm) {
        window.blazorMonaco.isWasm = isWasm;
    },

    //#endregion

    //#region Static methods

    colorize: async function (text, languageId, options) {
        let promise = monaco.editor.colorize(text, languageId, options);
        return await promise;
    },

    colorizeElement: async function (elementId, options) {
        let promise = monaco.editor.colorizeElement(document.getElementById(elementId), options);
        await promise;
    },

    colorizeModelLine: function (uriStr, lineNumber, tabSize) {
        let model = this.model.getModel(uriStr);
        if (model == null)
            return null;
        return monaco.editor.colorizeModelLine(model, lineNumber, tabSize);
    },

    create: function (id, options, override, dotnetRef) {
        if (typeof monaco === 'undefined') {
            console.error("BlazorMonaco: monaco is undefined. Please check that you have the script tag for editor.main.js in your html file");
            return;
        }

        if (options == null)
            options = {};

        let oldEditor = this.getEditor(id, true);
        if (oldEditor !== null) {
            if (options.value == null) {
                options.value = oldEditor.getValue();
            }

            window.blazorMonaco.editors.splice(window.blazorMonaco.editors.findIndex(h => h.id === id), 1);
            oldEditor.dispose();
        }

        if (options.lineNumbers == "function") {
            options.lineNumbers = (lineNumber) => {
                return this.getEditorLineNumber(id, lineNumber);
            };
        }

        let editor = monaco.editor.create(document.getElementById(id), options, override);
        window.blazorMonaco.editors.push({ id: id, editor: editor, dotnetRef: dotnetRef });
    },

    createDiffEditor: function (id, options, override, dotnetRef, dotnetRefOriginal, dotnetRefModified) {
        if (typeof monaco === 'undefined') {
            console.error("BlazorMonaco: monaco is undefined. Please check that you have the script tag for editor.main.js in your html file");
            return;
        }

        if (options == null)
            options = {};

        let oldEditor = this.getEditor(id, true);
        let oldModel = null;
        if (oldEditor !== null) {
            oldModel = oldEditor.getModel();

            window.blazorMonaco.editors.splice(window.blazorMonaco.editors.findIndex(h => h.id === id + "_original"), 1);
            window.blazorMonaco.editors.splice(window.blazorMonaco.editors.findIndex(h => h.id === id + "_modified"), 1);
            window.blazorMonaco.editors.splice(window.blazorMonaco.editors.findIndex(h => h.id === id), 1);
            oldEditor.dispose();
        }

        if (options.lineNumbers == "function") {
            options.lineNumbers = (lineNumber) => {
                return this.getEditorLineNumber(id, lineNumber);
            };
        }

        let editor = monaco.editor.createDiffEditor(document.getElementById(id), options, override);
        window.blazorMonaco.editors.push({ id: id, editor: editor, dotnetRef: dotnetRef });
        window.blazorMonaco.editors.push({ id: id + "_original", editor: editor.getOriginalEditor(), dotnetRef: dotnetRefOriginal });
        window.blazorMonaco.editors.push({ id: id + "_modified", editor: editor.getModifiedEditor(), dotnetRef: dotnetRefModified });

        if (oldModel !== null && oldModel?.original !== null && oldModel?.modified !== null)
            editor.setModel(oldModel);
    },

    createModel: function (value, language, uriStr) {
        //uri is the key; if no uri exists, create one
        if (uriStr == null || uriStr == "") {
            uriStr = "generatedUriKey_" + this.uuidv4();
        }

        let uri = monaco.Uri.parse(uriStr);
        let model = monaco.editor.createModel(value, language, uri);
        if (model == null)
            return null;

        return {
            id: model.id,
            uri: model.uri.toString()
        };
    },

    defineTheme: function (themeName, themeData) {
        monaco.editor.defineTheme(themeName, themeData);
    },

    getModel: function (uriStr) {
        let model = this.model.getModel(uriStr);
        if (model == null)
            return null;

        return {
            id: model.id,
            uri: model.uri.toString()
        };
    },

    getModels: function () {
        return monaco.editor.getModels().map(function (value) {
            return {
                id: value.id,
                uri: value.uri.toString()
            };
        });
    },

    remeasureFonts: function () {
        monaco.editor.remeasureFonts();
    },

    setModelLanguage: function (uriStr, languageId) {
        let model = this.model.getModel(uriStr);
        if (model == null)
            return;
        monaco.editor.setModelLanguage(model, languageId);
    },

    setModelMarkers: function (uriStr, owner, markers) {
        let model = this.model.getModel(uriStr);
        if (model == null)
            return;
        monaco.editor.setModelMarkers(model, owner, markers);
    },

    removeAllMarkers: function (owner) {
        monaco.editor.removeAllMarkers(owner);
    },

    getModelMarkers: function (filter) {
        let markers = monaco.editor.getModelMarkers(filter);
        return blazorMonaco.editor.convertUriToString(markers);
    },

    setTheme: function (theme) {
        monaco.editor.setTheme(theme);
    },

    //#endregion

    //#region Instance methods

    addAction: function (id, actionDescriptor) {
        let editorHolder = this.getEditorHolder(id);
        editorHolder.editor.addAction({
            id: actionDescriptor.id,
            label: actionDescriptor.label,
            keybindings: actionDescriptor.keybindings,
            precondition: actionDescriptor.precondition,
            keybindingContext: actionDescriptor.keybindingContext,
            contextMenuGroupId: actionDescriptor.contextMenuGroupId,
            contextMenuOrder: actionDescriptor.contextMenuOrder,
            run: function (editor, args) {
                editorHolder.dotnetRef.invokeMethodAsync("ActionCallback", actionDescriptor.id);
            }
        });
    },

    addCommand: function (id, keybinding, context) {
        let editorHolder = this.getEditorHolder(id);
        editorHolder.editor.addCommand(keybinding, function (args) {
            editorHolder.dotnetRef.invokeMethodAsync("CommandCallback", keybinding);
        }, context);
    },

    deltaDecorations: function (id, oldDecorations, newDecorations) {
        let editor = this.getEditor(id);
        return editor.deltaDecorations(oldDecorations, newDecorations);
    },

    removeDecorations: function (id, decorationsIds) {
        let editor = this.getEditor(id);
        return editor.removeDecorations(decorationsIds);
    },

    dispose: function (id) {
        let editor = this.getEditor(id);
        editor.dispose();
    },

    focus: function (id) {
        let editor = this.getEditor(id);
        editor.focus();
    },

    executeEdits: function (id, source, edits, endCursorState) {
        let editorHolder = this.getEditorHolder(id);
        if (endCursorState == "function") {
            if (window.blazorMonaco.isWasm) {
                endCursorState = (inverseEditOperations) => {
                    return editorHolder.dotnetRef.invokeMethod("ExecuteEditsCallback", inverseEditOperations);
                }
            }
            else {
                console.warn("BlazorMonaco: Editor.ExecuteEdits() supports endCursorState lambda only in Blazor Wasm apps. Please use the other ExecuteEdits override that takes List<Selection> instead.");
                endCursorState = null;
            }
        }
        return editorHolder.editor.executeEdits(source, edits, endCursorState);
    },

    getContainerDomNodeId: function (id) {
        let editor = this.getEditor(id);
        let containerNode = editor.getContainerDomNode();
        if (containerNode == null)
            return null;
        return containerNode.id;
    },

    getContentHeight: function (id) {
        let editor = this.getEditor(id);
        return editor.getContentHeight();
    },

    getContentWidth: function (id) {
        let editor = this.getEditor(id);
        return editor.getContentWidth();
    },

    getDomNodeId: function (id) {
        let editor = this.getEditor(id);
        let domeNode = editor.getDomNode();
        if (domeNode == null)
            return null;
        return domeNode.id;
    },

    getEditorType: function (id) {
        let editor = this.getEditor(id);
        return editor.getEditorType();
    },

    getInstanceModel: function (id) {
        let editor = this.getEditor(id);
        let model = editor.getModel();
        if (model == null)
            return null;

        return {
            id: model.id,
            uri: model.uri.toString()
        };
    },

    getInstanceDiffModel: function (id) {
        let editor = this.getEditor(id);
        let model = editor.getModel();
        if (model == null)
            return null;

        return {
            original: {
                id: model.original.id,
                uri: model.original.uri.toString()
            },
            modified: {
                id: model.modified.id,
                uri: model.modified.uri.toString()
            },
        };
    },

    getLayoutInfo: function (id) {
        let editor = this.getEditor(id);
        return editor.getLayoutInfo();
    },

    getOffsetForColumn: function (id, lineNumber, column) {
        let editor = this.getEditor(id);
        return editor.getOffsetForColumn(lineNumber, column);
    },

    getOption: function (id, optionId) {
        let editor = this.getEditor(id);
        return JSON.stringify(editor.getOption(optionId));
    },

    getOptions: function (id) {
        let editor = this.getEditor(id);
        return editor.getOptions()._values.map(function (value) {
            return JSON.stringify(value);
        });
    },

    getPosition: function (id) {
        let editor = this.getEditor(id);
        return editor.getPosition();
    },

    getRawOptions: function (id) {
        let editor = this.getEditor(id);
        return editor.getRawOptions();
    },

    getScrollHeight: function (id) {
        let editor = this.getEditor(id);
        return editor.getScrollHeight();
    },

    getScrollLeft: function (id) {
        let editor = this.getEditor(id);
        return editor.getScrollLeft();
    },

    getScrollTop: function (id) {
        let editor = this.getEditor(id);
        return editor.getScrollTop();
    },

    getScrollWidth: function (id) {
        let editor = this.getEditor(id);
        return editor.getScrollWidth();
    },

    getScrolledVisiblePosition: function (id, position) {
        let editor = this.getEditor(id);
        return editor.getScrolledVisiblePosition(position);
    },

    getSelection: function (id) {
        let editor = this.getEditor(id);
        return editor.getSelection();
    },

    getSelections: function (id) {
        let editor = this.getEditor(id);
        return editor.getSelections();
    },

    getTargetAtClientPoint: function (id, clientX, clientY) {
        let editor = this.getEditor(id);
        return editor.getTargetAtClientPoint(clientX, clientY);
    },

    getFontSizeAtPosition: function (id, position) {
        let editor = this.getEditor(id);
        return editor.getFontSizeAtPosition(position);
    },

    getTopForLineNumber: function (id, lineNumber, includeViewZones) {
        let editor = this.getEditor(id);
        return editor.getTopForLineNumber(lineNumber, includeViewZones);
    },

    getBottomForLineNumber: function (id, lineNumber) {
        let editor = this.getEditor(id);
        return editor.getBottomForLineNumber(lineNumber);
    },

    getTopForPosition: function (id, lineNumber, column) {
        let editor = this.getEditor(id);
        return editor.getTopForPosition(lineNumber, column);
    },

    getLineHeightForPosition: function (id, position) {
        let editor = this.getEditor(id);
        return editor.getLineHeightForPosition(position);
    },

    writeScreenReaderContent: function (id, reason) {
        let editor = this.getEditor(id);
        editor.writeScreenReaderContent(reason);
    },

    getValue: function (id, preserveBOM, lineEnding) {
        let editor = this.getEditor(id);
        let options = null;
        if (preserveBOM != null && lineEnding != null) {
            options = {
                preserveBOM: preserveBOM,
                lineEnding: lineEnding
            };
        }
        return editor.getValue(options);
    },

    getVisibleColumnFromPosition: function (id, position) {
        let editor = this.getEditor(id);
        return editor.getVisibleColumnFromPosition(position);
    },

    getVisibleRanges: function (id) {
        let editor = this.getEditor(id);
        return editor.getVisibleRanges();
    },

    hasTextFocus: function (id) {
        let editor = this.getEditor(id);
        return editor.hasTextFocus();
    },

    hasWidgetFocus: function (id) {
        let editor = this.getEditor(id);
        return editor.hasWidgetFocus();
    },

    layout: function (id, dimension, postponeRendering) {
        let editor = this.getEditor(id);
        editor.layout(dimension, postponeRendering);
    },

    pushUndoStop: function (id) {
        let editor = this.getEditor(id);
        return editor.pushUndoStop();
    },

    popUndoStop: function (id) {
        let editor = this.getEditor(id);
        return editor.popUndoStop();
    },

    render: function (id, forceRedraw) {
        let editor = this.getEditor(id);
        editor.render(forceRedraw);
    },

    revealLine: function (id, lineNumber, scrollType) {
        let editor = this.getEditor(id);
        editor.revealLine(lineNumber, scrollType);
    },

    revealLineInCenter: function (id, lineNumber, scrollType) {
        let editor = this.getEditor(id);
        editor.revealLineInCenter(lineNumber, scrollType);
    },

    revealLineInCenterIfOutsideViewport: function (id, lineNumber, scrollType) {
        let editor = this.getEditor(id);
        editor.revealLineInCenterIfOutsideViewport(lineNumber, scrollType);
    },

    revealLines: function (id, startLineNumber, endLineNumber, scrollType) {
        let editor = this.getEditor(id);
        editor.revealLine(startLineNumber, endLineNumber, scrollType);
    },

    revealLinesInCenter: function (id, startLineNumber, endLineNumber, scrollType) {
        let editor = this.getEditor(id);
        editor.revealLinesInCenter(startLineNumber, endLineNumber, scrollType);
    },

    revealLinesInCenterIfOutsideViewport: function (id, startLineNumber, endLineNumber, scrollType) {
        let editor = this.getEditor(id);
        editor.revealLinesInCenterIfOutsideViewport(startLineNumber, endLineNumber, scrollType);
    },

    revealPosition: function (id, position, scrollType) {
        let editor = this.getEditor(id);
        editor.revealPosition(position, scrollType);
    },

    revealPositionInCenter: function (id, position, scrollType) {
        let editor = this.getEditor(id);
        editor.revealPositionInCenter(position, scrollType);
    },

    revealPositionInCenterIfOutsideViewport: function (id, position, scrollType) {
        let editor = this.getEditor(id);
        editor.revealPositionInCenterIfOutsideViewport(position, scrollType);
    },

    revealRange: function (id, range, scrollType) {
        let editor = this.getEditor(id);
        editor.revealRange(range, scrollType);
    },

    revealRangeAtTop: function (id, range, scrollType) {
        let editor = this.getEditor(id);
        editor.revealRangeAtTop(range, scrollType);
    },

    revealRangeInCenter: function (id, range, scrollType) {
        let editor = this.getEditor(id);
        editor.revealRangeInCenter(range, scrollType);
    },

    revealRangeInCenterIfOutsideViewport: function (id, range, scrollType) {
        let editor = this.getEditor(id);
        editor.revealRangeInCenterIfOutsideViewport(range, scrollType);
    },

    setEventListener: function (id, eventName) {
        let editorHolder = this.getEditorHolder(id);
        let editor = editorHolder.editor;
        let dotnetRef = editorHolder.dotnetRef;

        let listener = function (e) {
            let eventJson = JSON.stringify(e);
            if (eventName === "OnDidChangeModel") {
                eventJson = JSON.stringify(blazorMonaco.editor.convertUriToString(e));
            }
            else if (eventName === "OnDidChangeConfiguration") {
                eventJson = JSON.stringify(e._values);
            }
            dotnetRef.invokeMethodAsync("EventCallback", eventName, eventJson);
        };

        switch (eventName) {
            case "OnDidCompositionEnd": editor.onDidCompositionEnd(listener); break;
            case "OnDidCompositionStart": editor.onDidCompositionStart(listener); break;
            case "OnContextMenu": editor.onContextMenu(listener); break;
            case "OnDidBlurEditorText": editor.onDidBlurEditorText(listener); break;
            case "OnDidBlurEditorWidget": editor.onDidBlurEditorWidget(listener); break;
            case "OnDidChangeConfiguration": editor.onDidChangeConfiguration(listener); break;
            case "OnDidChangeCursorPosition": editor.onDidChangeCursorPosition(listener); break;
            case "OnDidChangeCursorSelection": editor.onDidChangeCursorSelection(listener); break;
            case "OnWillChangeModel": editor.onWillChangeModel(listener); break;
            case "OnDidChangeModel": editor.onDidChangeModel(listener); break;
            case "OnDidChangeModelContent": editor.onDidChangeModelContent(listener); break;
            case "OnDidChangeModelDecorations": editor.onDidChangeModelDecorations(listener); break;
            case "OnDidChangeModelLanguage": editor.onDidChangeModelLanguage(listener); break;
            case "OnDidChangeModelLanguageConfiguration": editor.onDidChangeModelLanguageConfiguration(listener); break;
            case "OnDidChangeModelOptions": editor.onDidChangeModelOptions(listener); break;
            case "OnDidContentSizeChange": editor.onDidContentSizeChange(listener); break;
            case "OnDidDispose": editor.onDidDispose(listener); break;
            case "OnDidFocusEditorText": editor.onDidFocusEditorText(listener); break;
            case "OnDidFocusEditorWidget": editor.onDidFocusEditorWidget(listener); break;
            case "OnDidLayoutChange": editor.onDidLayoutChange(listener); break;
            case "OnDidPaste": editor.onDidPaste(listener); break;
            case "OnDidScrollChange": editor.onDidScrollChange(listener); break;
            case "OnKeyDown": editor.onKeyDown(listener); break;
            case "OnKeyUp": editor.onKeyUp(listener); break;
            case "OnMouseDown": editor.onMouseDown(listener); break;
            case "OnMouseLeave": editor.onMouseLeave(listener); break;
            case "OnMouseMove": editor.onMouseMove(listener); break;
            case "OnMouseUp": editor.onMouseUp(listener); break;
        }
    },

    setInstanceModel: function (id, uriStr) {
        let model = this.model.getModel(uriStr);
        if (model == null)
            return;

        let editor = this.getEditor(id);
        editor.setModel(model);
    },

    setInstanceDiffModel: function (id, model) {
        let original_model = this.model.getModel(model.original.uri);
        let modified_model = this.model.getModel(model.modified.uri);
        if (original_model == null || modified_model == null)
            return;

        let editor = this.getEditor(id);
        editor.setModel({
            original: original_model,
            modified: modified_model,
        });
    },

    setPosition: function (id, position) {
        let editor = this.getEditor(id);
        editor.setPosition(position);
    },

    setScrollLeft: function (id, newScrollLeft, scrollType) {
        let editor = this.getEditor(id);
        editor.setScrollLeft(newScrollLeft, scrollType);
    },

    setScrollPosition: function (id, newPosition, scrollType) {
        let editor = this.getEditor(id);
        editor.setScrollPosition(newPosition, scrollType);
    },

    hasPendingScrollAnimation: function (id) {
        let editor = this.getEditor(id);
        return editor.hasPendingScrollAnimation();
    },

    setScrollTop: function (id, newScrollTop, scrollType) {
        let editor = this.getEditor(id);
        editor.setScrollTop(newScrollTop, scrollType);
    },

    setSelection: function (id, selection) {
        let editor = this.getEditor(id);
        editor.setSelection(selection);
    },

    setSelections: function (id, selection) {
        let editor = this.getEditor(id);
        editor.setSelections(selection);
    },

    setValue: function (id, value) {
        let editor = this.getEditor(id);
        editor.setValue(value);
    },

    trigger: function (id, source, handlerId, payload) {
        let editor = this.getEditor(id);
        editor.trigger(source, handlerId, payload);
    },

    updateOptions: function (id, options) {
        let editor = this.getEditor(id);
        editor.updateOptions(options);
    },

    uuidv4: function () {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            let r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    },

    //#endregion

    //#region TextModel methods

    model: {
        getModel: function (uriStr) {
            let uri = monaco.Uri.parse(uriStr);
            if (uri == null)
                return null;
            return monaco.editor.getModel(uri);
        },

        getOptions: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.getOptions();
        },

        getVersionId: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.getVersionId();
        },

        getAlternativeVersionId: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.getAlternativeVersionId();
        },

        setValue: function (uriStr, newValue) {
            let model = this.getModel(uriStr);
            return model.setValue(newValue);
        },

        getValue: function (uriStr, eol, preserveBOM) {
            let model = this.getModel(uriStr);
            return model.getValue(eol, preserveBOM);
        },

        getValueLength: function (uriStr, eol, preserveBOM) {
            let model = this.getModel(uriStr);
            return model.getValueLength(eol, preserveBOM);
        },

        getValueInRange: function (uriStr, range, eol) {
            let model = this.getModel(uriStr);
            return model.getValueInRange(range, eol);
        },

        getValueLengthInRange: function (uriStr, range, eol) {
            let model = this.getModel(uriStr);
            return model.getValueLengthInRange(range, eol);
        },

        getCharacterCountInRange: function (uriStr, range, eol) {
            let model = this.getModel(uriStr);
            return model.getCharacterCountInRange(range, eol);
        },

        getLineCount: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.getLineCount();
        },

        getLineContent: function (uriStr, lineNumber) {
            let model = this.getModel(uriStr);
            return model.getLineContent(lineNumber);
        },

        getLineLength: function (uriStr, lineNumber) {
            let model = this.getModel(uriStr);
            return model.getLineLength(lineNumber);
        },

        getLinesContent: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.getLinesContent();
        },

        getEOL: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.getEOL();
        },

        getEndOfLineSequence: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.getEndOfLineSequence();
        },

        getLineMinColumn: function (uriStr, lineNumber) {
            let model = this.getModel(uriStr);
            return model.getLineMinColumn(lineNumber);
        },

        getLineMaxColumn: function (uriStr, lineNumber) {
            let model = this.getModel(uriStr);
            return model.getLineMaxColumn(lineNumber);
        },

        getLineFirstNonWhitespaceColumn: function (uriStr, lineNumber) {
            let model = this.getModel(uriStr);
            return model.getLineFirstNonWhitespaceColumn(lineNumber);
        },

        getLineLastNonWhitespaceColumn: function (uriStr, lineNumber) {
            let model = this.getModel(uriStr);
            return model.getLineLastNonWhitespaceColumn(lineNumber);
        },

        validatePosition: function (uriStr, position) {
            let model = this.getModel(uriStr);
            return model.validatePosition(position);
        },

        modifyPosition: function (uriStr, position, offset) {
            let model = this.getModel(uriStr);
            return model.modifyPosition(position, offset);
        },

        validateRange: function (uriStr, range) {
            let model = this.getModel(uriStr);
            return model.validateRange(range);
        },

        isValidRange: function (uriStr, range) {
            let model = this.getModel(uriStr);
            return model.isValidRange(range);
        },

        getOffsetAt: function (uriStr, position) {
            let model = this.getModel(uriStr);
            return model.getOffsetAt(position);
        },

        getPositionAt: function (uriStr, offset) {
            let model = this.getModel(uriStr);
            return model.getPositionAt(offset);
        },

        getFullModelRange: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.getFullModelRange();
        },

        isDisposed: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.isDisposed();
        },

        findMatches: function (uriStr, searchString, searchScope_or_searchOnlyEditableRange, isRegex, matchCase, wordSeparators, captureMatches, limitResultCount) {
            let model = this.getModel(uriStr);
            return model.findMatches(searchString, searchScope_or_searchOnlyEditableRange, isRegex, matchCase, wordSeparators, captureMatches, limitResultCount);
        },

        findNextMatch: function (uriStr, searchString, searchStart, isRegex, matchCase, wordSeparators, captureMatches) {
            let model = this.getModel(uriStr);
            return model.findNextMatch(searchString, searchStart, isRegex, matchCase, wordSeparators, captureMatches);
        },

        findPreviousMatch: function (uriStr, searchString, searchStart, isRegex, matchCase, wordSeparators, captureMatches) {
            let model = this.getModel(uriStr);
            return model.findPreviousMatch(searchString, searchStart, isRegex, matchCase, wordSeparators, captureMatches);
        },

        getLanguageId: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.getLanguageId();
        },

        getWordAtPosition: function (uriStr, position) {
            let model = this.getModel(uriStr);
            return model.getWordAtPosition(position);
        },

        getWordUntilPosition: function (uriStr, position) {
            let model = this.getModel(uriStr);
            return model.getWordUntilPosition(position);
        },

        deltaDecorations: function (uriStr, oldDecorations, newDecorations, ownerId) {
            let model = this.getModel(uriStr);
            return model.deltaDecorations(oldDecorations, newDecorations, ownerId);
        },

        getDecorationOptions: function (uriStr, id) {
            let model = this.getModel(uriStr);
            return model.getDecorationOptions(id);
        },

        getDecorationRange: function (uriStr, id) {
            let model = this.getModel(uriStr);
            return model.getDecorationRange(id);
        },

        getLineDecorations: function (uriStr, lineNumber, ownerId, filterOutValidation, filterFontDecorations) {
            let model = this.getModel(uriStr);
            let result = model.getLineDecorations(lineNumber, ownerId, filterOutValidation, filterFontDecorations);
            return blazorMonaco.editor.removeCircularReferences(result);
        },

        getLinesDecorations: function (uriStr, startLineNumber, endLineNumber, ownerId, filterOutValidation, filterFontDecorations) {
            let model = this.getModel(uriStr);
            let result = model.getLinesDecorations(startLineNumber, endLineNumber, ownerId, filterOutValidation, filterFontDecorations);
            return blazorMonaco.editor.removeCircularReferences(result);
        },

        getDecorationsInRange: function (uriStr, range, ownerId, filterOutValidation, filterFontDecorations, onlyMinimapDecorations, onlyMarginDecorations) {
            let model = this.getModel(uriStr);
            let result = model.getDecorationsInRange(range, ownerId, filterOutValidation, filterFontDecorations, onlyMinimapDecorations, onlyMarginDecorations);
            return blazorMonaco.editor.removeCircularReferences(result);
        },

        getAllDecorations: function (uriStr, ownerId, filterOutValidation, filterFontDecorations) {
            let model = this.getModel(uriStr);
            let result = model.getAllDecorations(ownerId, filterOutValidation, filterFontDecorations);
            return blazorMonaco.editor.removeCircularReferences(result);
        },

        getAllMarginDecorations: function (uriStr, ownerId) {
            let model = this.getModel(uriStr);
            let result = model.getAllMarginDecorations(ownerId);
            return blazorMonaco.editor.removeCircularReferences(result);
        },

        getOverviewRulerDecorations: function (uriStr, ownerId, filterOutValidation, filterFontDecorations) {
            let model = this.getModel(uriStr);
            let result = model.getOverviewRulerDecorations(ownerId, filterOutValidation, filterFontDecorations);
            return blazorMonaco.editor.removeCircularReferences(result);
        },

        getInjectedTextDecorations: function (uriStr, ownerId) {
            let model = this.getModel(uriStr);
            let result = model.getInjectedTextDecorations(ownerId);
            return blazorMonaco.editor.removeCircularReferences(result);
        },

        getCustomLineHeightsDecorations: function (uriStr, ownerId) {
            let model = this.getModel(uriStr);
            let result = model.getCustomLineHeightsDecorations(ownerId);
            return blazorMonaco.editor.removeCircularReferences(result);
        },

        normalizeIndentation: function (uriStr, str) {
            let model = this.getModel(uriStr);
            return model.normalizeIndentation(str);
        },

        updateOptions: function (uriStr, newOpts) {
            let model = this.getModel(uriStr);
            return model.updateOptions(newOpts);
        },

        detectIndentation: function (uriStr, defaultInsertSpaces, defaultTabSize) {
            let model = this.getModel(uriStr);
            return model.detectIndentation(defaultInsertSpaces, defaultTabSize);
        },

        pushStackElement: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.pushStackElement();
        },

        popStackElement: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.popStackElement();
        },

        pushEOL: function (uriStr, eol) {
            let model = this.getModel(uriStr);
            return model.pushEOL(eol);
        },

        applyEdits: function (uriStr, operations, computeUndoEdits) {
            let model = this.getModel(uriStr);
            return model.applyEdits(operations, computeUndoEdits);
        },

        setEOL: function (uriStr, eol) {
            let model = this.getModel(uriStr);
            return model.setEOL(eol);
        },

        undo: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.undo();
        },

        canUndo: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.canUndo();
        },

        redo: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.redo();
        },

        canRedo: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.canRedo();
        },

        dispose: function (uriStr) {
            let model = this.getModel(uriStr);
            return model.dispose();
        }
    }

    //#endregion
}

window.blazorMonaco.languages = {
    //#region Static methods

    registerHoverProvider: async function (language, hoverProviderRef) {
        await monaco.languages.registerHoverProvider(language, {
            provideHover: (model, position, cancellationToken, context) => {
                return hoverProviderRef.invokeMethodAsync("ProvideHover", decodeURI(model.uri.toString()), position, context);
            }
        });
    },

    registerCodeActionProvider: async function (language, codeActionProviderRef, metadata) {
        await monaco.languages.registerCodeActionProvider(language, {
            provideCodeActions: (model, range, context, cancellationToken) => {
                return codeActionProviderRef.invokeMethodAsync("ProvideCodeActions", decodeURI(model.uri.toString()), range, context)
                    .then(result => {
                        (result.actions || []).forEach(action => {
                            (action.edit.edits || []).forEach(edit => {
                                if (edit.resource != null)
                                    edit.resource = monaco.Uri.parse(edit.resource)
                                if (edit.oldResource != null)
                                    edit.oldResource = monaco.Uri.parse(edit.oldResource)
                                if (edit.newResource != null)
                                    edit.newResource = monaco.Uri.parse(edit.newResource)
                            });
                        });
                        result.dispose = () => { };
                        return result;
                    });
            },
            resolveCodeAction: (codeAction, cancellationToken) => {
                return codeActionProviderRef.invokeMethodAsync("ResolveCodeAction", codeAction);
            }
        }, metadata);
    },

    registerDocumentFormattingEditProvider: async function (language, displayName, documentFormattingEditProviderRef) {
        await monaco.languages.registerDocumentFormattingEditProvider(language, {
            displayName: displayName,
            provideDocumentFormattingEdits: (model, options, cancellationToken) => {
                return documentFormattingEditProviderRef.invokeMethodAsync("ProvideDocumentFormattingEdits", decodeURI(model.uri.toString()), options);
            }
        });
    },

    registerCompletionItemProvider: async function (language, triggerCharacters, completionItemProviderRef) {
        await monaco.languages.registerCompletionItemProvider(language, {
            triggerCharacters: triggerCharacters,
            provideCompletionItems: (model, position, context, cancellationToken) => {
                return completionItemProviderRef.invokeMethodAsync("ProvideCompletionItems", decodeURI(model.uri.toString()), position, context);
            },
            resolveCompletionItem: (completionItem, cancellationToken) => {
                return completionItemProviderRef.invokeMethodAsync("ResolveCompletionItem", completionItem);
            }
        });
    }

    //#endregion
}
