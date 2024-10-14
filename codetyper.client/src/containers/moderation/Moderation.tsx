import { Link } from "react-router-dom";

const Moderation: React.FC = () => {
  return (
      <div className="row is-center">
          <Link to={`/snippets/randomRequest`} className="col-3 button primary" style={{ textDecoration: 'none' }}>Review snippet requests</Link>
          <Link to={`/tasks/randomRequest`} className="col-3 button primary" style={{ textDecoration: 'none' }}>Review task requests</Link>
      </div>
  );
}

export default Moderation;