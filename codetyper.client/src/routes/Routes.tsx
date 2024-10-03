import { createBrowserRouter } from "react-router-dom";
import App from "../App";
import AuthContainer from "../containers/auth/AuthContainer";

export const router = createBrowserRouter([
    {
        path: "/",
        element: <App />,
        children: [
            { path: "/auth", element: <AuthContainer /> },
        ]
    }
]);
