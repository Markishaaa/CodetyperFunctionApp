import React, { useState } from "react";
import { useParams } from "react-router-dom";
import { addCodeSnippet, CodeSnippetData } from "../../../services/SnippetService";
import { toast } from "react-toastify";
import Language from "../../../models/Language";
import SelectLanguage from "../../language/languageSelect/SelectLanguage";
import CodeMirrorEditor from "../../../components/CodeMirrorEditor";

const AddSnippet: React.FC = () => {
    const { taskId } = useParams<{ taskId: string }>();
    const creatorId: string = sessionStorage.getItem("userId")!;

    const [content, setContent] = useState("");

    const [language, setLanguage] = useState<Language>();

    const resetForm = () => {
        setContent("");
    }

    const handleLanguageChange = (selectedLanguage: Language) => {
        setLanguage(selectedLanguage);
        console.log(language);
    }

    const handleAddCodeSnippet = async () => {
        if (!language || content === "") {
            toast.error("Please fill in the form.");
            return;
        }

        const snippetData: CodeSnippetData = {
            content: content,
            languageName: language!.name,
            taskId: Number(taskId),
            creatorId: creatorId,
        };

        try {
            const result = await addCodeSnippet(snippetData);
            if (result.success) {
                toast.success(result.message);
                resetForm();
            } else {
                toast.error(result.message);
            }
        } catch (error) {
            if (error instanceof Error) {
                toast.error(error.message);
            } else {
                toast.error("An unknown error occurred.");
            }
        }
    };

    return (
        <div>
            <h2>Add Code Snippet</h2>
            <SelectLanguage onLanguageChange={handleLanguageChange}></SelectLanguage>

            <CodeMirrorEditor
                value={content}
                onChange={setContent}
                language={language ? language.name.toLowerCase() : ""}
            />

            <button onClick={handleAddCodeSnippet}>Add Snippet</button>
        </div>
    );
};

export default AddSnippet;