import React from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import "./User.scss";
import DataSource from "components/DataSource";

interface Props {
  actionFetch(
    type: string,
    action: string,
    queryParams?: { email?: string; password?: string },
    errorHandler?: any
  ): any;
}
interface State {}

class User extends React.Component<Props, State> {
  render() {
    const { actionFetch } = this.props;
    return (
      <DataSource dataSource={async () => await actionFetch("users", "CURRENT")}>
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
