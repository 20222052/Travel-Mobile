import React from 'react';
import { Link } from 'react-router-dom';

/**
 * Content Header Component for Admin Pages
 */
const ContentHeader = ({ title, subtitle, breadcrumbs = [] }) => {
  return (
    <section className="content-header">
      <h1>
        {title}
        {subtitle && <small>{subtitle}</small>}
      </h1>
      {breadcrumbs.length > 0 && (
        <ol className="breadcrumb">
          {breadcrumbs.map((item, index) => (
            <li key={index} className={index === breadcrumbs.length - 1 ? 'active' : ''}>
              {item.link ? (
                <Link to={item.link}>
                  {item.icon && <i className={`fa ${item.icon}`}></i>} {item.label}
                </Link>
              ) : (
                <>
                  {item.icon && <i className={`fa ${item.icon}`}></i>} {item.label}
                </>
              )}
            </li>
          ))}
        </ol>
      )}
    </section>
  );
};

export default ContentHeader;
