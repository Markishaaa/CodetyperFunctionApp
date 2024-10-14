import React, { useEffect, useState } from "react";
import { getAllShownTasks } from "../../../services/TaskService";
import { toast } from "react-toastify";
import './taskList.css';
import { Link } from "react-router-dom";
import { CodingTask } from "../../../models/CodingTask";
import Markdown from "react-markdown";

const PAGE_SIZE = 3;

const TaskList: React.FC = () => {
    const [tasks, setTasks] = useState<CodingTask[] | undefined>([]);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        const fetchTasks = async (page: number) => {
            setLoading(true);
            try {
                const result = await getAllShownTasks(page, PAGE_SIZE);
                if (result.success) {
                    setTasks(result.tasks);
                    setTotalPages(result.totalPages || 1); // ensure at least 1 page
                } else {
                    toast.error(result.message);
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

        fetchTasks(currentPage);
    }, [currentPage]);

    const handlePageChange = (newPage: number) => {
        if (newPage >= 1 && newPage <= totalPages) {
            setCurrentPage(newPage);
        }
    };

    const getShortDescription = (description: string) => {
        const match = description.split('\n')[0].trim();

        return match ? match : description.trim();
    };

    return (
        <div className="task-list-container">
            <h4>Task List</h4>

            {loading ? (
                <p>Loading tasks...</p>
            ) : (
                <div>
                    {tasks!.length === 0 ? (
                        <p>No tasks found.</p>
                    ) : (
                        <div className="task-list">
                                    {tasks!.map((task, index) => (
                                        <Link to={`/tasks/${task.id}`} state={task} key={index} style={{textDecoration: 'none'}}>
                                    <div className="task-box" key={index}>
                                        <h4>{task.name}</h4>
                                                <Markdown>{getShortDescription(task.description)}</Markdown>
                                    </div>
                                </Link>
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

export default TaskList;
