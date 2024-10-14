import { Outlet, useLocation, useNavigate } from 'react-router';
import './styles/App.css';
import NavbarComponent from './components/navbar/NavbarComponent';
import { logoutUser } from './services/AuthService';
import 'react-toastify/dist/ReactToastify.css';
import { ToastContainer } from 'react-toastify';
import TypingGame from './components/typingGame/TypingGame';
import TaskList from './containers/task/taskList/TaskList';
import { useState } from 'react';

function App() {
    const location = useLocation();
    const isFrontPage = location.pathname === "/";

    const [resetTrigger, setResetTrigger] = useState<number>(0);
    const navigate = useNavigate();

    const handleResetAndNavigate = () => {
        setResetTrigger(prev => prev + 1);
        navigate('/');
    };

    return (
        <div className="App">
            <div className="content flex-column">
                <NavbarComponent onLogout={logoutUser} onReset={handleResetAndNavigate} />
                <div className="padding">
                    {isFrontPage ? (<>
                        <TypingGame resetTrigger={resetTrigger}></TypingGame>
                        <hr className="hr"></hr>
                        <TaskList></TaskList>
                    </>) : (<>
                        <Outlet />
                     </>)}
                </div>
            </div>
            <ToastContainer
                className="toast"
                position="bottom-center"
                autoClose={5000}
                newestOnTop={true}
                closeOnClick
                rtl={false}
                pauseOnHover
                closeButton={false}
            />
        </div>
    );
}

export default App;