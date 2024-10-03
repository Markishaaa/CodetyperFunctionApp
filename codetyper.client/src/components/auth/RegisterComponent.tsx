import { useState } from "react";
import UserData, { registerUser } from "../../services/AuthService";
import './auth-component.css';

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
    const isPasswordInvalid = credentials.password.length < 6;

    const handleRegistration = async () => {
        try {
            const success = await registerUser(credentials);
            if (success) {
                console.log('Registration successful');
            }
        } catch (error) {
            if (error instanceof Error)
                console.error('Error during registration:', error.message);
        }
    };

    return (
        <div className="wrapper crt">
            <div className="container">
                <input type="email" name="register-email" required placeholder="Email" value={credentials.email} onChange={(e) => handleInputChange(e, 'email')} />
                {isEmailInvalid && <span className="error-indicator">X</span>}
                <input type="text" name="register-username" required placeholder="Username" value={credentials.username} onChange={(e) => handleInputChange(e, 'username')} />
                {<span className="error-indicator">X</span>}
                <input type="password" name="register-password" autoCapitalize="new-password" required placeholder="Password" value={credentials.password} onChange={(e) => handleInputChange(e, 'password')} />
                {isPasswordInvalid && <span className="error-indicator">X</span>}
                <button onClick={handleRegistration} disabled={isEmailInvalid || isPasswordInvalid}>Register</button>
            </div>
        </div>
    );
};

export default RegisterComponent;