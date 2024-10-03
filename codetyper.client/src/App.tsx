import { Outlet } from 'react-router';
import './styles/App.css';
import NavbarComponent from './components/navbar/NavbarComponent';
import { logoutUser } from './services/AuthService';

function App() {
    return (
        <div className="App">
            <div className="content">
                <NavbarComponent onLogout={logoutUser} />
                <Outlet />
            </div>
        </div>
    );
}

export default App;