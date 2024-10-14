import React, { useEffect, useState } from "react";
import { getUserById } from "../../../services/UserService";
import CodeMirrorEditor from "../../../components/CodeMirrorEditor";
import "./snippetDetails.css";

interface Snippet {
    content: string;
    languageName: string;
    creatorId: string;
}

interface SnippetWithCreatorProps {
    snippet: Snippet;
}

const SnippetDetails: React.FC<SnippetWithCreatorProps> = ({ snippet }) => {
    const [creator, setCreator] = useState<string | null>(null);

    useEffect(() => {
        const fetchCreator = async () => {
            const fetchedUser = await getUserById(snippet.creatorId);
            if (fetchedUser)
                setCreator(fetchedUser.username);
        };

        fetchCreator();
    }, [snippet.creatorId]);

    return (
        <div className="snippet-box col">
            <CodeMirrorEditor
                value={snippet.content}
                onChange={() => { }}
                language={snippet.languageName.toLowerCase()}
                readOnly="nocursor"
            />
            <div className="info-box row">
                <p className="col">Language: {snippet.languageName}</p>
                <p className="col is-right">Added by: {creator || "Loading..."}</p>
            </div>
        </div>
    );
};

export default SnippetDetails;
