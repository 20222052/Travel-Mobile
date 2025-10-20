import React from 'react';

/**
 * Box component - AdminLTE style box
 */
export const Box = ({ 
  title, 
  type = 'default', 
  loading = false, 
  collapsible = false,
  tools = null,
  children 
}) => {
  const [collapsed, setCollapsed] = React.useState(false);

  return (
    <div className={`box box-${type}`}>
      {title && (
        <div className="box-header with-border">
          <h3 className="box-title">{title}</h3>
          <div className="box-tools pull-right">
            {tools}
            {collapsible && (
              <button 
                type="button" 
                className="btn btn-box-tool"
                onClick={() => setCollapsed(!collapsed)}
              >
                <i className={`fa fa-${collapsed ? 'plus' : 'minus'}`}></i>
              </button>
            )}
          </div>
        </div>
      )}
      <div className={`box-body ${collapsed ? 'hide' : ''}`}>
        {loading ? (
          <div className="overlay">
            <i className="fa fa-refresh fa-spin"></i>
          </div>
        ) : (
          children
        )}
      </div>
    </div>
  );
};

/**
 * Small Box component - AdminLTE info box
 */
export const SmallBox = ({ 
  title, 
  value, 
  icon, 
  color = 'aqua', 
  link, 
  linkText = 'More info' 
}) => {
  return (
    <div className={`small-box bg-${color}`}>
      <div className="inner">
        <h3>{value}</h3>
        <p>{title}</p>
      </div>
      <div className="icon">
        <i className={`ion ${icon}`}></i>
      </div>
      {link && (
        <a href={link} className="small-box-footer">
          {linkText} <i className="fa fa-arrow-circle-right"></i>
        </a>
      )}
    </div>
  );
};

/**
 * Info Box component
 */
export const InfoBox = ({ 
  title, 
  value, 
  icon, 
  color = 'aqua', 
  progress = null 
}) => {
  return (
    <div className="info-box">
      <span className={`info-box-icon bg-${color}`}>
        <i className={`fa ${icon}`}></i>
      </span>
      <div className="info-box-content">
        <span className="info-box-text">{title}</span>
        <span className="info-box-number">{value}</span>
        {progress !== null && (
          <>
            <div className="progress">
              <div className="progress-bar" style={{ width: `${progress}%` }}></div>
            </div>
            <span className="progress-description">{progress}% Increase in 30 Days</span>
          </>
        )}
      </div>
    </div>
  );
};

/**
 * Alert component
 */
export const Alert = ({ 
  type = 'info', 
  dismissible = true, 
  icon = null,
  children,
  onClose
}) => {
  const [visible, setVisible] = React.useState(true);

  const handleClose = () => {
    setVisible(false);
    if (onClose) onClose();
  };

  if (!visible) return null;

  return (
    <div className={`alert alert-${type} ${dismissible ? 'alert-dismissible' : ''}`}>
      {dismissible && (
        <button 
          type="button" 
          className="close" 
          onClick={handleClose}
        >
          <span aria-hidden="true">&times;</span>
        </button>
      )}
      {icon && <i className={`icon fa ${icon}`}></i>}
      {children}
    </div>
  );
};

/**
 * Loading Spinner
 */
export const LoadingSpinner = ({ size = 'md', text = 'Loading...' }) => {
  const sizeClass = {
    sm: 'fa-lg',
    md: 'fa-2x',
    lg: 'fa-3x'
  };

  return (
    <div className="text-center">
      <i className={`fa fa-spinner fa-spin ${sizeClass[size]}`}></i>
      {text && <p className="text-muted">{text}</p>}
    </div>
  );
};

/**
 * Modal component
 */
export const Modal = ({ 
  show, 
  onHide, 
  title, 
  size = 'md',
  footer = null,
  children 
}) => {
  if (!show) return null;

  return (
    <div className="modal fade in" style={{ display: 'block' }}>
      <div className={`modal-dialog modal-${size}`}>
        <div className="modal-content">
          <div className="modal-header">
            <button type="button" className="close" onClick={onHide}>
              <span aria-hidden="true">&times;</span>
            </button>
            <h4 className="modal-title">{title}</h4>
          </div>
          <div className="modal-body">
            {children}
          </div>
          {footer && (
            <div className="modal-footer">
              {footer}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
