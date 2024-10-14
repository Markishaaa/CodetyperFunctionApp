import { createBrowserRouter } from "react-router-dom";
import App from "../App";
import AuthContainer from "../containers/auth/AuthContainer";
import AddTaskContainer from "../containers/task/addTask/AddTaskContainer";
import AddLanguageContainer from "../containers/language/addLanguage/AddLanguageContainer";

export const router = createBrowserRouter([
    {
        path: "/",
        element: <App />,
        children: [
            { path: "/auth", element: <AuthContainer /> },
            { path: "/addLanguage", element: <AddLanguageContainer /> },
            { path: "/addTask", element: <AddTaskContainer /> },
        ]
    }
]);
