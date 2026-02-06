import { BrowserRouter, Routes, Route } from 'react-router-dom';
import RootLayout from '@/app/layout';
import CalendarEditorPage from '@/pages/CalendarEditorPage';
import AvailabilityFinderPage from '@/pages/AvailabilityFinderPage';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<RootLayout />}>
          <Route index element={<CalendarEditorPage />} />
          <Route path="find" element={<AvailabilityFinderPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
