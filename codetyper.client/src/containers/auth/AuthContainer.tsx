import RegisterComponent from "../../components/auth/RegisterComponent";
import LoginComponent from "../../components/auth/LoginComponent";
import './auth-container.css';

const AuthContainer: React.FC = () => {
    return (
        <div className="wrapper">
            <div className="item"><RegisterComponent></RegisterComponent></div>
            <div className="item"><LoginComponent></LoginComponent></div>
        </div>
    );
};

export default AuthContainer;