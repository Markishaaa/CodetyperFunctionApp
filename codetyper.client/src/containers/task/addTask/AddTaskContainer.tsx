import { useState } from "react";
import { addTask } from "../../../services/TaskService";
import MarkdownIt from 'markdown-it';
import MdEditor from 'react-markdown-editor-lite';
import 'react-markdown-editor-lite/lib/index.css';

const mdParser = new MarkdownIt();

const AddTaskContainer: React.FC = () => {
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");

    const handleAddTask = async () => {
        const processedDescription = processMarkdown(description);

        const taskDto = { name: name, description: processedDescription };
        const result = await addTask(taskDto);
        console.log(result);
    };

    const processMarkdown = (markdown: string) => {
        return markdown.replace(/{{IMAGE_URL}}/g, '![Alt text](image-url.jpg)');
    }

    return (
        <div>
            <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
            />
            <MdEditor
                value={description}
                onChange={({ text }) => setDescription(text)}
                renderHTML={(text) => mdParser.render(text)}
            />
            <button onClick={handleAddTask}>Add task</button>
        </div>
    );
}

export default AddTaskContainer;