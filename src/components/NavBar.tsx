import React from "react";

type NavBarProps = {
  onSave: () => void;            
  onReload: () => void;          
  setDrawType: (type: "Point" | "LineString" | "Polygon") => void;
  enableNavigation: () => void;  
  toggleList: () => void;        
};

const NavBar: React.FC<NavBarProps> = ({
  onSave,
  onReload,
  setDrawType,
  enableNavigation,
  toggleList,
}) => {
  const buttonStyle: React.CSSProperties = {
    padding: "8px 14px",
    border: "none",
    borderRadius: "6px",
    background: "rgba(255, 255, 255, 0.7)",
    color: "#111",
    cursor: "pointer",
    fontWeight: 500,
    transition: "all 0.2s ease",
  };

  const buttonHoverStyle: React.CSSProperties = {
    background: "rgba(255, 255, 255, 1)",
    transform: "scale(1.05)",
  };

  return (
    <nav
      style={{
        position: "fixed",
        top: 0,
        left: 0,
        width: "100%",
        background: "rgba(20, 19, 19, 0.43)",
        backdropFilter: "blur(5px)",
        padding: "10px 20px",
        zIndex: 500,
        display: "flex",
        alignItems: "center",
        gap: "10px",
      }}
    >
      {["Point", "LineString", "Polygon"].map((type) => (
        <button
          key={type}
          style={buttonStyle}
          onMouseOver={(e) =>
            Object.assign(e.currentTarget.style, buttonHoverStyle)
          }
          onMouseOut={(e) => Object.assign(e.currentTarget.style, buttonStyle)}
          onClick={() => setDrawType(type as "Point" | "LineString" | "Polygon")}
        >
          {type === "LineString" ? "Line" : type}
        </button>
      ))}
      <button
        style={buttonStyle}
        onMouseOver={(e) => Object.assign(e.currentTarget.style, buttonHoverStyle)}
        onMouseOut={(e) => Object.assign(e.currentTarget.style, buttonStyle)}
        onClick={enableNavigation}
      >
        Gez
      </button>
      <button
        style={buttonStyle}
        onMouseOver={(e) => Object.assign(e.currentTarget.style, buttonHoverStyle)}
        onMouseOut={(e) => Object.assign(e.currentTarget.style, buttonStyle)}
        onClick={onSave}
      >
        Kaydet
      </button>
      <button
        style={buttonStyle}
        onMouseOver={(e) => Object.assign(e.currentTarget.style, buttonHoverStyle)}
        onMouseOut={(e) => Object.assign(e.currentTarget.style, buttonStyle)}
        onClick={toggleList}
      >
        Liste
      </button>
      <button
        style={buttonStyle}
        onMouseOver={(e) => Object.assign(e.currentTarget.style, buttonHoverStyle)}
        onMouseOut={(e) => Object.assign(e.currentTarget.style, buttonStyle)}
        onClick={onReload}
      >
        Yenile
      </button>
    </nav>
  );
};

export default NavBar;
