/* eslint-disable @typescript-eslint/no-explicit-any */
import { Link } from 'react-router-dom';
import './navbar-component.css';

const NavbarComponent: React.FC<{ onLogout: () => void, onReset: any }> = ({ onLogout, onReset }) => {
    const username = sessionStorage.getItem('username');
    const role = sessionStorage.getItem('role');

    const targetRoute = role && role !== "User" ? "/moderation" : "";

    return (
        <>
            <nav className="nav">
                <div className="nav-left">
                    {username ? (
                        <>
                            {role!=="User" &&
                                <Link to="/addLanguage" className="text-white">
                                    L
                                </Link>
                            }
                            <Link to="/addTask" className="text-white">
                                T
                            </Link>
                        </>
                    ) : (<></>)}
                </div>

                <div className="nav-center">
                    <Link to="/" className="brand text-success" onClick={onReset}>
                        codetyper
                    </Link>
                </div>

                <div className="nav-right">
                    {username ? (
                        <>
                            <Link to={ targetRoute } className="text-white">
                                {username} - {role}
                            </Link>
                            <Link to="/auth" className="text-white" onClick={onLogout}>
                                logout
                            </Link>
                        </>
                    ) : (
                        <>
                            <Link to="/auth" className="text-white">
                                login
                            </Link>
                        </>
                    )}
                    </div>
            </nav>
            <hr />
        </>
    );
};

export default NavbarComponent;
