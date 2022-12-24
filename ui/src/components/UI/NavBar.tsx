import React, { Fragment } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { AppProps } from "index";
import { setTheme } from "utils";
import { getTheme } from "utils/setTheme";
import ReactTooltip from "react-tooltip";
import "./NavBar.scss";

export class Navbar extends React.Component<AppProps & { listConfig?: any }, {}> {
  state = { modal: null, modalClassName: "" };
  onItemClick = (id, navbarRowId) => {
    const { listConfig = [] } = this.props;
    const configOptions = listConfig[navbarRowId];
    const item = configOptions.length > 0 ? configOptions.find((item) => item.id === id) : null;
    if (item && item.onClick) item.onClick();
    else this.setState({ modal: item, modalClassName: item.modalClassName || "" });
  };
  modalClose = () => this.setState({ modal: null, modalClassName: "" });
  render() {
    const navbar = [
      [],
      [
        {
          id: "brand",
          tooltip: "Home",
          className: "brand center cursor-pointer",
          img: <img src="/favicon.png" alt="Brand" />,
          onClick: () => this.props.history.push("/"),
        },
      ],
      [
        {
          id: "home",
          tooltip: "Home",
          icon: "home",
          onClick: () => this.props.history.push("/"),
        },
        {
          id: "user",
          tooltip: "User",
          icon: "user-circle",
          onClick: () => this.props.history.push("/user"),
        },
        {
          id: "dark",
          tooltip: "Dark Mode",
          icon: "moon",
          onClick: () => setTheme(getTheme() === "light" ? "dark" : "light"),
        },
        {
          id: "logout",
          tooltip: "Logout",
          icon: "sign-out-alt",
          componentHeader: "Logout",
          onClick: () => this.props.onLogoutHandler(),
        },
      ],
    ];
    const { listConfig = navbar } = this.props;
    return (
      <div id="navbar">
        <div className={`navbar-list`}>
          <ReactTooltip id="navbar-tooltip" place="bottom" effect="solid" className="tooltip" />
          <div key="menu" className="navbar-list-container">
            <ul className={"menu flex grow"}>
              {listConfig.map((section, index) => {
                return (
                  <div key={`menu ${index}`} className={`grow-even row`}>
                    {section.map((item) => {
                      const { id, icon, title, className = null, hide, tooltip, img } = item;
                      if (hide) return null;
                      return (
                        <Fragment key={id}>
                          <li
                            data-tip={tooltip}
                            data-for="navbar-tooltip"
                            className={className}
                            onClick={() => {
                              if (item.onClick) item.onClick(id, index);
                            }}
                          >
                            <div className="icon-title">
                              {img ? <span>{img}</span> : null}
                              {icon && (
                                <FontAwesomeIcon icon={icon} style={{ fontSize: "1.5em" }} />
                              )}
                              {title ? <span>{title}</span> : null}
                            </div>
                          </li>
                        </Fragment>
                      );
                    })}
                  </div>
                );
              })}
            </ul>
          </div>
        </div>
      </div>
    );
  }
}
