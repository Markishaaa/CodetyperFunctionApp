import { useEffect, useState } from "react";
import { getRandomSnippetRequest, SnippetCreator, SnippetTaskDetails } from "../../services/SnippetService";
import { toast } from "react-toastify";
import { CodeSnippet } from "../../models/CodeSnippet";
import Markdown from "react-markdown";
import SnippetDetails from "../snippet/snippetDetails/SnippetDetails";
import { ApiResponse } from "../../services/AuthService";
import { acceptSnippetRequest, denySnippetRequest } from "../../services/ModerationService";

const ReviewSnippet: React.FC = () => {
    const [snippet, setSnippet] = useState<CodeSnippet>();
    const [task, setTask] = useState<SnippetTaskDetails>();
    const [creator, setCreator] = useState<SnippetCreator>();
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string>();

    const fetchRandomSnippet = async () => {
        try {
            const response = await getRandomSnippetRequest();
            if (response.snippet) {
                setSnippet(response.snippet);
                setTask(response.task);
                setCreator(response.creator);
                console.log(response);
            } else {
                setError(response.message);
            }
        } catch (err) {
            setError('Failed to fetch random snippet: ' + err);
            toast.error('Failed to fetch random snippet.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchRandomSnippet();
    }, []);

    const handleAccept = async () => {
        if (!task) return;

        try {
            const response: ApiResponse = await acceptSnippetRequest(snippet!.id);
            if (response.success) {
                toast.success("Snippet accepted successfully!");
                fetchRandomSnippet();
            } else {
                toast.error(response.message || "Failed to accept the snippet.");
            }
        } catch (error) {
            console.log(error);
            toast.error("An error occurred while accepting the snippet.");
        }
    };

    const handleDeny = async () => {
        if (!task) return;

        const reason = prompt("Please provide a reason for denying the snippet:");
        if (!reason) return;

        try {
            const response: ApiResponse = await denySnippetRequest(snippet!.id, reason);
            if (response.success) {
                toast.success("Snippet denied and archived.");
                fetchRandomSnippet();
            } else {
                toast.error(response.message || "Failed to deny the snippet.");
            }
        } catch (error) {
            console.log(error);
            toast.error("An error occurred while denying the snippet.");
        }
    };

    if (loading) {
        return <p>Loading...</p>;
    }

    if (error) {
        return <p>{error}</p>;
    }

    return (
        <div>
            {task && (
                <div>
                    <h3>{task.name}</h3>
                    <Markdown>{task.description}</Markdown>
                </div>
            )}
            {snippet && creator && (
                <div>
                    <SnippetDetails snippet={snippet}></SnippetDetails>
                    <div className="moderation-buttons">
                        <button className="button accept" onClick={handleAccept}>
                            Accept Snippet
                        </button>
                        <button className="button deny" onClick={handleDeny}>
                            Deny Snippet
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default ReviewSnippet;