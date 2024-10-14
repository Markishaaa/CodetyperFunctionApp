/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable @typescript-eslint/no-explicit-any */
import React from 'react';
import { Controlled as CodeMirror } from 'react-codemirror2';
import '../assets/CodeMirrorImports';

interface CodeMirrorEditorProps {
    value: string;
    language: string;
    onChange: (value: string) => void;
    readOnly?: boolean | "nocursor";
    handleKeyDown?: any;
    handleOnPaste?: any;
}

const CodeMirrorEditor: React.FC<CodeMirrorEditorProps> = ({ value, language, onChange, readOnly, handleKeyDown, handleOnPaste }) => {
    const handleKeydownDefault = (_editor: never, _event: KeyboardEvent) => { };
    const handleBeforePasteDefault = (_editor: never, _event: ClipboardEvent) => { };

    const mapLanguage = () => {
        if (language.toLowerCase() === "c++")
            return "text/x-c++src";
        else if (language.toLowerCase() === "c")
            return "text/x-csrc";
        else if (language.toLowerCase() === "java")
            return "text/x-java";
        else if (language.toLowerCase() === "c#")
            return "text/x-csharp";
        else
            return language.toLowerCase();
    }

    return (
        <CodeMirror
            value={value}
            options={{
                mode: mapLanguage(),
                theme: 'dracula',
                lineNumbers: true,
                lineWrapping: true,
                readOnly: readOnly,
                indentWithTabs: true,
                matchBrackets: true,        
                autoCloseBrackets: true,     
                smartIndent: true,           
                indentUnit: 2,
                extraKeys: {
                    "Ctrl-Space": "autocomplete"
                },
                gutters: ["CodeMirror-linenumbers", "CodeMirror-foldgutter"],
                foldGutter: true,
                tabSize: 2
            }}
            onBeforeChange={(_editor, _data, value) => {
                onChange(value);
            }}
            onKeyDown={handleKeyDown ? handleKeyDown : handleKeydownDefault}
            onPaste={handleOnPaste ? handleOnPaste : handleBeforePasteDefault}

        />
    );
};

export default CodeMirrorEditor;
