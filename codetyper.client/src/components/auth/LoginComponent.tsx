import { useState } from "react";
import './auth-component.css';
import { useNavigate } from "react-router";
import UserData, { loginUser } from "../../services/AuthService";

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
            const success = await loginUser(credentials);
            if (success) {
                navigate('/');
                window.location.reload();
            }
        } catch (error) {
            if (error instanceof Error)
                console.error('Error during login:', error.message);
        }
    };

    return (
        <div className="wrapper">
            <div className="container">
                <input type="text" name="login-username" autoComplete="username" placeholder="Username" value={credentials.username} onChange={(e) => handleInputChange(e, 'username')} />
                <input type="password" name="login-password" autoComplete="current-password" placeholder="Password" value={credentials.password} onChange={(e) => handleInputChange(e, 'password')} />
                <button onClick={handleLogin}>Login</button>
            </div>
        </div>
    );
};

export default LoginComponent;