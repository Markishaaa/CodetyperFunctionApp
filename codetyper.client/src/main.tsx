import ReactDOM from 'react-dom/client'
// import './styles/index.css'
import 'chota';
import { RouterProvider } from 'react-router-dom'
import { router } from './routes/Routes.tsx'

ReactDOM.createRoot(document.getElementById('root')!).render(
    //<React.StrictMode>
        <RouterProvider router={router} />
    //</React.StrictMode>
)
