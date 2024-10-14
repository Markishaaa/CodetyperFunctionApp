import { useState } from "react";
import { addTask } from "../../../services/TaskService";
import { toast } from "react-toastify";
import "./addTask.css";
import MDEditor from "@uiw/react-md-editor";

const AddTask: React.FC = () => {
    const creatorId: string = sessionStorage.getItem("userId")!;
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");

    const clearForm = () => {
        setName("");
        setDescription("");
    }

    const handleAddTask = async () => {
        if (description === "" || name === "") {
            toast.error("Please fill in the form.");
            return;
        }

        const processedDescription = processMarkdown(description);

        const taskDto = { creatorId: creatorId, description: processedDescription, name: name };

        try {
            const result = await addTask(taskDto);
            if (result.success) {
                toast.success(result.message);
                clearForm();
            } else
                toast.error(result.message);
        } catch (error) {
            if (error instanceof Error)
                toast.error(error.message);
            else
                toast.error("An unknown error occurred.");
        }
    };

    const processMarkdown = (markdown: string) => {
        return markdown.replace(/{{IMAGE_URL}}/g, '![Alt text](image-url.jpg)');
    }

    return (
        <div className="content">
            <div className="input-group">
                <label htmlFor="task-name">Task Name</label>
                <input
                    id="task-name"
                    type="text"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    placeholder="Enter task name"
                />
            </div>

            <div className="md-editor-group">
                <label htmlFor="task-desc">Task Description</label>
                <MDEditor className="md-editor"
                    value={description}
                    onChange={(value) => setDescription(value || "")}
                    data-color-mode="dark"
                />
                <MDEditor.Markdown style={{ whiteSpace: 'pre-wrap' }} source={description} />
            </div>

            <button className="button primary" onClick={handleAddTask}>Add task</button>
        </div>
    );
}

export default AddTask;