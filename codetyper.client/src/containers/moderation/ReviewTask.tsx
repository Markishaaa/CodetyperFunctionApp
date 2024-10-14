import React, { useEffect, useState } from "react";
import { toast } from "react-toastify";
import { CodingTask } from "../../models/CodingTask";
import User from "../../models/User";
import { getRandomTask } from "../../services/TaskService";
import { acceptTaskRequest, denyTaskRequest } from "../../services/ModerationService";
import { ApiResponse } from "../../services/AuthService";
import Markdown from "react-markdown";

const ReviewTask: React.FC = () => {
    const [task, setTask] = useState<CodingTask | null>(null);
    const [creator, setCreator] = useState<User | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [count, setCount] = useState<number>(0);
    const [error, setError] = useState<string | null>(null);


    const fetchRandomTask = async () => {
        setLoading(true);
        try {
            const result = await getRandomTask();
            console.log(result);
            if (result) {
                setTask(result.Task);
                setCreator(result.Creator);
                setCount(result.Count);
            } else {
                setError("No task requests to show.");
            }
        } catch (err) {
            if (err instanceof Error) {
                setError(err.message);
            } else {
                setError("An unknown error occurred.");
            }
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchRandomTask();
    }, []);

    const handleAccept = async () => {
        if (!task) return;

        try {
            const response: ApiResponse = await acceptTaskRequest(task.id!);
            if (response.success) {
                toast.success("Task accepted successfully!");
                fetchRandomTask();
            } else {
                toast.error(response.message || "Failed to accept the task.");
            }
        } catch (error) {
            console.log(error);
            toast.error("An error occurred while accepting the task.");
        }
    };

    const handleDeny = async () => {
        if (!task) return;

        const reason = prompt("Please provide a reason for denying the task:");
        if (!reason) return;

        try {
            const response: ApiResponse = await denyTaskRequest(task.id!, reason);
            if (response.success) {
                toast.success("Task denied and archived.");
                fetchRandomTask();
            } else {
                toast.error(response.message || "Failed to deny the task.");
            }
        } catch (error) {
            console.log(error);
            toast.error("An error occurred while denying the task.");
        }
    };

    if (loading) {
        return <p>Loading task...</p>;
    }

    if (error) {
        return <p>Error: {error}</p>;
    }

    return (
        <div className="review-task-container">
            {task && creator ? (
                <div>
                    <h2>Review Task</h2>
                    <p>Total Tasks Left: {count}</p>
                    <div className="task-box">
                        <h3>{task.name}</h3>
                        <hr></hr>
                        <Markdown>{task.description}</Markdown>
                    </div>
                    <div className="creator-info">
                        <h4>Created by: {creator.username}</h4>
                    </div>

                    <div className="moderation-buttons">
                        <button className="button accept" onClick={handleAccept}>
                            Accept Task
                        </button>
                        <button className="button deny" onClick={handleDeny}>
                            Deny Task
                        </button>
                    </div>
                </div>
            ) : (
                <p>No task to review.</p>
            )}
        </div>
    );
};

export default ReviewTask;
