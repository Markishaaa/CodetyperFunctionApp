import React, { useState, useEffect } from "react";
import { CodeSnippetDetails, getSnippetsByTask } from "../../../services/SnippetService";
import { toast } from "react-toastify";
import Language from "../../../models/Language";
import SelectLanguage from "../../language/languageSelect/SelectLanguage";
import SnippetDetails from "../snippetDetails/SnippetDetails";
import "./snippetList.css";

interface SnippetListProps {
    taskId: number;
    languageName?: string;
}

const PAGE_SIZE = 3;

const SnippetList: React.FC<SnippetListProps> = ({ taskId, languageName }) => {
    const [snippets, setSnippets] = useState<CodeSnippetDetails[]>([]);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [loading, setLoading] = useState(false);
    const [language, setLanguage] = useState<Language>();

    const handleLanguageChange = (selectedLanguage: Language) => {
        setLanguage(selectedLanguage);
    }

    useEffect(() => {
        const fetchSnippets = async (page: number) => {
            setLoading(true);
            try {
                let languageName = "";
                if (language)
                    languageName = language.name;
                const result = await getSnippetsByTask(taskId, page, PAGE_SIZE, languageName);
                if (result != null) {
                    setSnippets(result.snippets);
                    setTotalPages(result.totalPages || 1);
                }
            } catch (error) {
                if (error instanceof Error) {
                    toast.error(error.message);
                } else {
                    toast.error("An unknown error occurred.");
                }
            } finally {
                setLoading(false);
            }
        };

        fetchSnippets(currentPage);
    }, [taskId, languageName, currentPage, language]);

    const handlePageChange = (newPage: number) => {
        if (newPage >= 1 && newPage <= totalPages) {
            setCurrentPage(newPage);
        }
    };

    if (loading) {
        return <div>Loading snippets...</div>;
    }

    return (
        <div className="task-list-container">
            <div className="row">
                <hr className="hr"></hr>
                {snippets.length === 0 ?
                    <h3 className="col-9">No {languageName} snippets found.</h3>
                    :
                    <h3 className="col-9">Snippet List</h3>
                }
                
                <div className="col-3"><SelectLanguage onLanguageChange={handleLanguageChange}></SelectLanguage></div>
            </div>

            {loading && snippets.length > 0 ? (
                <p>Loading snippets...</p>
            ) : (
                <div>
                    {snippets!.length === 0 ? (
                        <p>No snippets found.</p>
                        ) : (
                        <div className="snippet-list">
                            {snippets!.map((snippet, index) => (
                                <SnippetDetails snippet={snippet} key={index}></SnippetDetails>
                            ))}
                        </div>
                    )}

                    {totalPages > 1 && (
                        <div className="pagination row is-center">
                            <button
                                className="button col"
                                onClick={() => handlePageChange(currentPage - 1)}
                                disabled={currentPage === 1}
                            >
                                Previous
                            </button>
                            <span className="page-info col text-center">
                                Page {currentPage} of {totalPages}
                            </span>
                            <button
                                className="button col"
                                onClick={() => handlePageChange(currentPage + 1)}
                                disabled={currentPage === totalPages}
                            >
                                Next
                            </button>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
};

export default SnippetList;
