import { useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import User from "../../../models/User";
import { getUserById } from "../../../services/UserService";
import SnippetList from "../../snippet/snippetList/SnippetList";
import Markdown from 'react-markdown'


const TaskDetails = () => {
    const location = useLocation();
    const { id, name, description, creatorId } = location.state || {};
    const [creator, setCreator] = useState<User | null>(null);

    useEffect(() => {
        const fetchUser = async () => {
            const fetchedUser = await getUserById(creatorId);
            setCreator(fetchedUser);
        };

        fetchUser();
    }, [creatorId]);

    if (!name) {
        return <h1>No task details available.</h1>;
    }

    const getRoleInitial = (role: string | undefined) => {
        if (role === undefined)
            return;

        if (role)
            return role.charAt(0);
    }

    return (
        <div>
            { }
            <div className="row nav-right">
                <Link to={`/addSnippet/${id}`} className="col-3 button primary" style={{ textDecoration: 'none' }}>Add snippet</Link>
            </div>
            <div className="row">
                <h1 className="col-9">{name}</h1>
                <p className="col-3 is-right">Added by: {creator?.username} ({getRoleInitial(creator?.roleName)})</p>
                
            </div>
            <Markdown>{description}</Markdown>
        </div>
    );
};

export default TaskDetails;