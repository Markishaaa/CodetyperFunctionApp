import { useLocation } from "react-router-dom";
import SnippetList from "../../snippet/snippetList/SnippetList";
import TaskDetails from "./TaskDetails";

const TaskDetailsContainer = () => {
    const location = useLocation();
    const { id } = location.state || {};

    return (
        <>
            <TaskDetails></TaskDetails>
            <SnippetList taskId={id}></SnippetList>
        </>
    );
}

export default TaskDetailsContainer;