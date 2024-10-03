import { Link } from 'react-router-dom';
import './navbar-component.css';

const NavbarComponent: React.FC<{ onLogout: () => void }> = ({ onLogout }) => {
    const username = sessionStorage.getItem('username');
    const role = sessionStorage.getItem('role');

    return (
        <div>
            <div className="links">
                <Link to="/" className="title spacer">
                    codetyper
                </Link>

                {username ? (
                    <>
                        <Link to="/addLanguage" className="links spacer">
                            L
                        </Link>
                        <Link to="/addTask" className="links spacer">
                            T
                        </Link>
                        <Link to="/addSnippet" className="links spacer">
                            S
                        </Link>
                    </>
                ) : (<></>)}
            
            <div className="right">
                {username ? (
                    <>
                        <Link to="/auth" className="links spacer right" onClick={onLogout}>
                            logout
                        </Link>
                            <div className="links spacer right">{username} - {role}</div>
                    </>
                ) : (
                    <>
                        <Link to="/auth" className="links spacer right">
                            login
                        </Link>
                    </>
                )}
                </div>
                
            </div>
            <hr />
        </div>
    );
};

export default NavbarComponent;
