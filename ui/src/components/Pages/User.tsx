import React from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import "./User.scss";
import DataSource from "components/DataSource";
import { AppProps } from "index";
import { URLS } from "constants/URLS";

interface State {}

class User extends React.Component<AppProps, State> {
  render() {
    const { urlFetch } = this.props;
    return (
      <DataSource dataSource={async () => await urlFetch(URLS.user.CURRENT)}>
        {({ data: user }: { data: any }) => (
          <div id="user-profile" className="col grow">
            <div className="border margin padding10">
              <div className="row">
                <div className="grow pull-right">
                  <FontAwesomeIcon icon="user" className="avatar" />
                </div>
                <div className="grow">
                  <h1 className="grow">{user?.name}</h1>
                </div>
              </div>
            </div>
          </div>
        )}
      </DataSource>
    );
  }
}

export default User;
