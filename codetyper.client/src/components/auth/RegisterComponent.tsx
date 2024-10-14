import { useState } from "react";
import UserData, { registerUser } from "../../services/AuthService";
import './auth-component.css';
import { toast } from "react-toastify";

const RegisterComponent: React.FC = () => {
    const [credentials, setCredentials] = useState<UserData>({
        username: '',
        password: '',
        email: '',
    });

    const handleInputChange = (
        e: React.ChangeEvent<HTMLInputElement>,
        field: keyof UserData
    ) => {
        setCredentials({ ...credentials, [field]: e.target.value });
    };

    const isEmailInvalid = !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(credentials.email!);
    const isPasswordInvalid = credentials.password.length < 8;
    const isUsernameInvalid = credentials.username.length < 3;

    const handleRegistration = async () => {
        try {
            const result = await registerUser(credentials);
            if (result.success)
                toast.success(result.message);
            else
                toast.error(result.message);
        } catch (error) {
            if (error instanceof Error)
                toast.error(error.message);
            else
                toast.error("An unknown error occurred.");
        }
    };

    return (
        <div className="container">
            <input type="email" className="button outline text-white" style={{ width: `20rem` }} name="register-email" required placeholder="Email" value={credentials.email} onChange={(e) => handleInputChange(e, 'email')} />
            {isEmailInvalid && credentials.email && <span className="error-indicator">X</span>}
            <input type="text" className="button outline text-white" style={{ marginLeft: 0, width: `20rem` }} name="register-username" required placeholder="Username" value={credentials.username} onChange={(e) => handleInputChange(e, 'username')} />
            {isUsernameInvalid && credentials.username && <span className="error-indicator">X</span>}
            <input type="password" className="button outline text-white" style={{ marginLeft: 0, width: `20rem` }} name="register-password" autoCapitalize="new-password" required placeholder="Password" value={credentials.password} onChange={(e) => handleInputChange(e, 'password')} />
            {isPasswordInvalid && credentials.password && <span className="error-indicator">X</span>}
            <button onClick={handleRegistration} disabled={isEmailInvalid || isPasswordInvalid}>Register</button>
        </div>
    );
};

export default RegisterComponent;