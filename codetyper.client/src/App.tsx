import { Outlet } from 'react-router';
import './styles/App.css';
import NavbarComponent from './components/navbar/NavbarComponent';
import { logoutUser } from './services/AuthService';
import 'react-toastify/dist/ReactToastify.css';
import { ToastContainer } from 'react-toastify';

function App() {
    return (
        <div className="App">
            <div className="content">
                <NavbarComponent onLogout={logoutUser} />
                <Outlet />
                <ToastContainer
                    position="bottom-center"
                    autoClose={5000}
                    newestOnTop={true}
                    closeOnClick
                    rtl={false}
                    pauseOnHover
                />
            </div>
        </div>
    );
}

export default App;