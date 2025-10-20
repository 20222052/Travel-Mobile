import React from 'react';

const AdminFooter = () => {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="main-footer">
      <div className="pull-right hidden-xs">
        <b>Version</b> 1.0.0
      </div>
      <strong>
        Copyright &copy; {currentYear}{' '}
        <a href="https://travel.com">Travel Admin</a>.
      </strong>{' '}
      All rights reserved.
    </footer>
  );
};

export default AdminFooter;
