import { useState } from "react";
import './auth-component.css';
import { useNavigate } from "react-router";
import UserData, { loginUser } from "../../services/AuthService";
import { toast } from "react-toastify";

const LoginComponent: React.FC = () => {
    const [credentials, setCredentials] = useState<UserData>({
        username: '',
        password: ''
    });
    const navigate = useNavigate();

    const handleInputChange = (
        e: React.ChangeEvent<HTMLInputElement>,
        field: keyof UserData
    ) => {
        setCredentials({ ...credentials, [field]: e.target.value });
    };

    const handleLogin = async () => {
        try {
            const result = await loginUser(credentials);
            if (result.success) {
                navigate('/');
                window.location.reload();
                toast.success(result.message);
            } else
                toast.error(result.message);
        } catch (error) {
            if (error instanceof Error)
                toast.error(error.message);
            else
                toast.error("An unknown error occurred.");
        }
    };

    return (
        <div className="wrapper">
            <div className="container row">
                <input type="text" className="button outline text-white row" style={{ width: `20rem` }} name="login-username" autoComplete="username" placeholder="Username" value={credentials.username} onChange={(e) => handleInputChange(e, 'username')} />
                <input type="password" className="button outline text-white row is-marginless" style={{ width: `20rem` }} name="login-password" autoComplete="current-password" placeholder="Password" value={credentials.password} onChange={(e) => handleInputChange(e, 'password')} />
                <button className="button primary" disabled={!credentials.username || !credentials.password} onClick={handleLogin}>Login</button>
            </div>
        </div>
    );
};

export default LoginComponent;