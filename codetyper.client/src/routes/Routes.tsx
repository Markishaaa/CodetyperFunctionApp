/* eslint-disable react-refresh/only-export-components */
import { createBrowserRouter, Navigate } from "react-router-dom";
import App from "../App";
import AuthContainer from "../containers/auth/AuthContainer";
import AddTask from "../containers/task/addTask/AddTask";
import AddLanguage from "../containers/language/addLanguage/AddLanguage";
import AddSnippet from "../containers/snippet/addSnippet/AddSnippet";
import Moderation from "../containers/moderation/Moderation";
import ReviewSnippet from "../containers/moderation/ReviewSnippet";
import ReviewTask from "../containers/moderation/ReviewTask";
import TaskDetailsContainer from "../containers/task/taskDetails/TaskDetailsContainer";

const role = sessionStorage.getItem("role");

interface ProtectedRouteProps {
    element: React.ReactNode;
}

const ProtectedUserRoute: React.FC<ProtectedRouteProps> = ({ element }) => {
    return role ? element : <Navigate to="/auth" replace />
}

const ProtectedStaffRoute: React.FC<ProtectedRouteProps> = ({ element }) => {
    return (role && role !== "User") ? element : <Navigate to="/auth" replace />
}

export const router = createBrowserRouter([
    {
        path: "/",
        element: <App />,
        children: [
            { path: "/auth", element: <AuthContainer /> },
            { path: "/tasks/:taskId", element: <TaskDetailsContainer></TaskDetailsContainer> },
            
            { path: "/addTask", element: <ProtectedUserRoute element={<AddTask />} /> },
            { path: "/addSnippet/:taskId", element: <ProtectedUserRoute element={<AddSnippet />} /> },

            { path: "/addLanguage", element: <ProtectedStaffRoute element={<AddLanguage />} /> },
            { path: "/moderation", element: <ProtectedStaffRoute element={<Moderation />} /> },
            { path: "/snippets/randomRequest", element: <ProtectedStaffRoute element={<ReviewSnippet />} /> },
            { path: "/tasks/randomRequest", element: <ProtectedStaffRoute element={<ReviewTask />} /> }
            
        ]
    }
]);
